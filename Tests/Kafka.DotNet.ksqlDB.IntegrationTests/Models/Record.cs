using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.Models
{
  public record Record
  {
    [IgnoreByInserts]
    public long RowTime { get; set; }
  }
}