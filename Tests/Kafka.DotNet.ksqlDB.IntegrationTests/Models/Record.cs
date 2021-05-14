using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.Models
{
  public record Record
  {
    [Ignore]
    public long RowTime { get; set; }
  }
}