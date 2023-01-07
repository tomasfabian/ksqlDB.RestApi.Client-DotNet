using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace Joins.Model.Orders;

record Shipment
{
  [Key]
  public int? Id { get; set; }
}
