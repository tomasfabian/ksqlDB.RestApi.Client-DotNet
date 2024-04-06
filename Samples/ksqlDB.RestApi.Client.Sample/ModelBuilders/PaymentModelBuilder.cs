using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.Samples.ModelBuilders;

public class PaymentModelBuilder
{
  private readonly string ksqlDbUrl = "http://localhost:8088";

  public async Task InitModelAndCreateStreamAsync(CancellationToken cancellationToken = default)
  {
    ModelBuilder builder = new();

    builder.Entity<Account>()
      .HasKey(c => c.Id)
      .Property(b => b.Balance);

    builder.Entity<Payment>()
      .HasKey(c => c.Id)
      .Property(b => b.Amount)
      .Decimal(precision: 10, scale: 2);

    var httpClient = new HttpClient
    {
      BaseAddress = new Uri(ksqlDbUrl)
    };
    var httpClientFactory = new HttpClientFactory(httpClient);
    IKSqlDbRestApiClient restApiProvider = new KSqlDbRestApiClient(httpClientFactory, builder);
    restApiProvider = ConfigureRestApiClientWithServicesCollection(new ServiceCollection(), builder);

    var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 1);

    var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
    var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
  }

  private IKSqlDbRestApiClient ConfigureRestApiClientWithServicesCollection(ServiceCollection serviceCollection, ModelBuilder builder)
  {
    serviceCollection
      .AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
      {
        c.UseKSqlDb(ksqlDbUrl);

        c.ReplaceHttpClient<ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory, ksqlDB.RestApi.Client.KSql.RestApi.Http.HttpClientFactory>(_ => { })
        .AddHttpMessageHandler(_ => new Program.DebugHandler());
      })
      .AddSingleton(builder);

    var provider = serviceCollection.BuildServiceProvider();

    var restApiClient = provider.GetRequiredService<IKSqlDbRestApiClient>();

    return restApiClient;
  }
}

record Payment
{
  public string Id { get; set; } = null!;
  public decimal Amount { get; set; }
  public string Description { get; set; } = null!;
}

record Account
{
  public string Id { get; set; } = null!;
  public decimal Balance { get; set; }
}
