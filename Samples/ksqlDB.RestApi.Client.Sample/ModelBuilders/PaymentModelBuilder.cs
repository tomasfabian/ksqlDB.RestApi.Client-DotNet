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

    modelBuilder.Entity<Payment>()
      .Property(b => b.Description)
      .HasColumnName("desc");

    string header = "abc";
    modelBuilder.Entity<PocoWithHeader>()
      .Property(c => c.Header)
      .WithHeader(header);

    var restApiProvider = ConfigureRestApiClientWithServicesCollection(new ServiceCollection(), modelBuilder);

    var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 1);

    var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
    var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
    Console.WriteLine(content);

    entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Account), partitions: 1)
    {
      Replicas = 1
    };
    await restApiProvider.CreateTableAsync<Account>(entityCreationMetadata, true, cancellationToken);
    await responseMessage.Content.ReadAsStringAsync(cancellationToken);

    var entityCreationMetadata2 = new EntityCreationMetadata(kafkaTopic: nameof(PocoWithHeader), partitions: 1)
    {
      Replicas = 1
    };
    await restApiProvider.CreateStreamAsync<PocoWithHeader>(entityCreationMetadata2, true, cancellationToken);
  }

  private IKSqlDbRestApiClient ConfigureRestApiClientWithServicesCollection(ServiceCollection serviceCollection, ModelBuilder builder)
  {
    serviceCollection
      .AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
      {
        c.UseKSqlDb(ksqlDbUrl);

        c.ReplaceHttpClient<KSql.RestApi.Http.IHttpClientFactory, KSql.RestApi.Http.HttpClientFactory>(_ => { })
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
  public byte[] Secret { get; set; } = null!;
}

record PocoWithHeader
{
  public byte[] Header { get; init; } = null!;
}
