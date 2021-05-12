using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Tests.Pocos
{
  internal class Transaction : Record
  {
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
  }
}