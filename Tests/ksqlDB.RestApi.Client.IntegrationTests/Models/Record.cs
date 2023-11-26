using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDb.RestApi.Client.IntegrationTests.Models;

public record Record
{
  [IgnoreByInserts]
  public long RowTime { get; set; }
}