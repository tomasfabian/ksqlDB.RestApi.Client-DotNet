using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Clauses
{
  public interface IAsClause
  {
    ICreateStatement<T> As<T>(string entityName = null);
  }
}