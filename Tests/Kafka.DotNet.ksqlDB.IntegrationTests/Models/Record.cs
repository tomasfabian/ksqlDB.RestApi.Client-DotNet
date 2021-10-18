using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.Api.Client.IntegrationTests.Models
{
  public record Record
  {
    [IgnoreByInserts]
    public long RowTime { get; set; }
  }
}