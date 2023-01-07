namespace Joins.Model.Orders;

class Order
{
  public int OrderId { get; set; }
  public int PaymentId { get; set; }
  public int ShipmentId { get; set; }
}
