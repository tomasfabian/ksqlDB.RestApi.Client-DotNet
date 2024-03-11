using ksqlDB.RestApi.Client.KSql.Linq.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;

#nullable enable
public interface IAsClause
{
  ICreateStatement<T> As<T>(string? entityName = null);
}
