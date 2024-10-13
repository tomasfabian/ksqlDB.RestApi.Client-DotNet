using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDb.RestApi.Client.IntegrationTests.Models;

public record Record
{
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Ignore]
  [PseudoColumn]
  public long RowTime { get; set; }
}
