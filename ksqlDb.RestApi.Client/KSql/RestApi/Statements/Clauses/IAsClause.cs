using ksqlDB.RestApi.Client.KSql.Linq.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses
{
  public interface IAsClause
  {
    ICreateStatement<T> As<T>(string entityName = null);
  }
}