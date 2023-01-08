using ksqlDB.RestApi.Client.KSql.Query;

namespace Statements.Model;

public class Person : Record
{
  public string Name { get; set; } = null!;
  public Address? Address { get; set; }
}