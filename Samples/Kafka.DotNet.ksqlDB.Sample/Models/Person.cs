using Kafka.DotNet.ksqlDB.KSql.Query;

namespace ksqlDB.Api.Client.Samples.Models
{
  public class Person : Record
  {
    public string Name { get; set; }
    public Address Address { get; set; }
  }
}