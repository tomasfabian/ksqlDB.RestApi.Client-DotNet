using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  public class Record
  {
    [IgnoreByInserts]
    public long RowTime { get; set; }
  }
}