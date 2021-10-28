using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace ksqlDB.Api.Client.Samples.Joins
{
  public class Joins
  {
    public Joins()
    {
      var ksqlDbUrl = @"http:\\localhost:8088";

      var context = new KSqlDBContext(ksqlDbUrl);

      var query = (from o in context.CreateQueryStream<Order>()
          join p1 in Source.Of<Payment>() on o.PaymentId equals p1.Id
          join s1 in Source.Of<Shipment>() on o.ShipmentId equals s1.Id into gj
          from sa in gj.DefaultIfEmpty()
          select new
                 {
                   orderId = o.OrderId,
                   shipmentId = sa.Id,
                   paymentId = p1.Id,
                 })
        .Take(5);
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
    public int Id { get; set; }
  }

  record Shipment
  {
    public int Id { get; set; }
  }
}
