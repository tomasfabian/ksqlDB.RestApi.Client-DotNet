using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  internal class StatementContext : QueryContext
  {
    internal string EntityName { get; set; }
    internal string Statement { get; set; }
    internal CreationType CreationType { get; set; }
    internal KSqlEntityType KSqlEntityType { get; set; }
  }
}