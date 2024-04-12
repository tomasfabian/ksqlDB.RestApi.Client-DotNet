using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.Samples.ModelBuilders;

public class PaymentModelBuilder
{
  private readonly string ksqlDbUrl = "http://localhost:8088";

  public async Task InitModelAndCreateStreamAsync(CancellationToken cancellationToken = default)
  {
    ModelBuilder modelBuilder = new();

    modelBuilder.Entity<Account>()
      .HasKey(c => c.Id)
      .Property(b => b.Secret)
      .Ignore();

    modelBuilder.Entity<Payment>()
      .HasKey(c => c.Id)
      .Property(b => b.Amount)
      .Decimal(precision: 10, scale: 2);

    var restApiProvider = ConfigureRestApiClientWithServicesCollection(new ServiceCollection(), modelBuilder);

    var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 1);

    var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
    var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

    entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Account), partitions: 1)
    {
      Replicas = 1
    };
    responseMessage = await restApiProvider.CreateTableAsync<Account>(entityCreationMetadata, true, cancellationToken);
    content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
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
  public string Secret { get; set; } = null!;
}
