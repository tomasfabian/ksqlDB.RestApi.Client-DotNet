using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.RestApi.Client.Samples.Models;

public class Person : Record
{
  public string Name { get; set; } = null!;
  public Address? Address { get; set; }
}