using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  public class Record
  {
    [Ignore]
    public long RowTime { get; set; }
  }
}