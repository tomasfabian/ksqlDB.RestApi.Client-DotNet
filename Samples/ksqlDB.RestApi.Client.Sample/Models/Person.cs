using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.Api.Client.Samples.Models;

public class Person : Record
{
  public string Name { get; set; } = null!;
  public Address? Address { get; set; }
}