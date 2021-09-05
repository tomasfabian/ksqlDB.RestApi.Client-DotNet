using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Sample.Models
{
  public class Person : Record
  {
    public string Name { get; set; }
    public Address Address { get; set; }
  }
}