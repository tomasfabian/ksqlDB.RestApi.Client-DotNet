using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

internal class StatementContext : QueryContext
{
  internal string? EntityName { get; set; }
  internal string Statement { get; set; } = null!;
  internal CreationType CreationType { get; set; }
  internal KSqlEntityType KSqlEntityType { get; set; }
}
