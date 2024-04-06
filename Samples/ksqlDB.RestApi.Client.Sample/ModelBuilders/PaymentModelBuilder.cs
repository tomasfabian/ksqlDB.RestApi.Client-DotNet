using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.Samples.Model;

public class PaymentModelBuilder
{
  public static async Task InitModelAndCreateStreamAsync(CancellationToken cancellationToken = default)
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
      BaseAddress = new Uri("http://localhost:8088")
    };
    var httpClientFactory = new HttpClientFactory(httpClient);
    var restApiProvider = new KSqlDbRestApiClient(httpClientFactory, builder);

    var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 1);

    var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
    var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
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
