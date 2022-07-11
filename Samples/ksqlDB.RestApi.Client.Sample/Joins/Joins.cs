using System;
using System.Linq;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.Api.Client.Samples.Joins;

public class Joins
{
  private readonly IKSqlDbRestApiClient restApiClient;
  private readonly IKSqlDBContext context;

  public Joins(IKSqlDbRestApiClient restApiClient, IKSqlDBContext context)
  {
    this.restApiClient = restApiClient ?? throw new ArgumentNullException(nameof(restApiClient));
    this.context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task SubscribeAsync()
  {
    var entityCreationMetadata = new EntityCreationMetadata
    {
      KafkaTopic = nameof(Order) + "-Join",
      Partitions = 1
    };

    var response = await restApiClient.CreateStreamAsync<Order>(entityCreationMetadata, ifNotExists: true);
    var r = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    response = await restApiClient.CreateTableAsync<Payment>(entityCreationMetadata with { KafkaTopic = nameof(Payment) + "-Join" }, ifNotExists: true);
    response = await restApiClient.CreateTableAsync<Shipment>(entityCreationMetadata with { KafkaTopic = nameof(Shipment) + "-Join" }, ifNotExists: true);

    var value = new Foo { Prop = 42 };

    var query = (from o in context.CreateQueryStream<Order>()
                 join p1 in Source.Of<Payment>() on o.PaymentId equals p1.Id
                 join s1 in Source.Of<Shipment>() on o.ShipmentId equals s1.Id into gj
                 from sa in gj.DefaultIfEmpty()
                 select new
                 {
                   value,
                   orderId = o.OrderId,
                   shipmentId = sa.Id,
                   paymentId = p1.Id,
                 })
      .Take(5);

    string ksql = query.ToQueryString();

    using var subscription = query
      .Subscribe(c =>
      {
        Console.WriteLine($"{nameof(Order.OrderId)}: {c.orderId}");

        Console.WriteLine($"{nameof(Order.PaymentId)}: {c.paymentId}");

        if (c.shipmentId.HasValue)
          Console.WriteLine($"{nameof(Order.ShipmentId)}: {c.shipmentId}");

      }, error =>
      {
        Console.WriteLine(error.Message);
      });

    var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
    var payment = new Payment { Id = 1 };
    var shipment = new Shipment { Id = 1 };

    response = await restApiClient.InsertIntoAsync(order);
    r = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

    response = await restApiClient.InsertIntoAsync(payment);

    await Task.Delay(TimeSpan.FromMilliseconds(250));
    response = await restApiClient.InsertIntoAsync(shipment);
  }
}

class Order
{
  public int OrderId { get; set; }
  public int PaymentId { get; set; }
  public int ShipmentId { get; set; }
}

class Payment
{
  [Key]
  public int Id { get; set; }
}

record Shipment
{
  [Key]
  public int? Id { get; set; }
}

struct Foo
{
  public int Prop { get; set; }
}
