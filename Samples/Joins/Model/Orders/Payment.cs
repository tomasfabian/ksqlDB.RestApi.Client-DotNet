using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace Joins.Model.Orders;

class Payment
{
  [Key]
  public int Id { get; set; }
}
