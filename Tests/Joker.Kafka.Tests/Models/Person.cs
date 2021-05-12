using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Tests.Pocos
{
  public class Person : Record
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }
  }
}