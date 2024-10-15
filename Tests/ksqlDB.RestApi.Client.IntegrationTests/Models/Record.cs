using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDb.RestApi.Client.IntegrationTests.Models;

public record Record
{
  [PseudoColumn]
  [IgnoreInDDL]
  public long RowTime { get; set; }

  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Ignore]
  [PseudoColumn]
  public long? RowOffset { get; set; }
}
