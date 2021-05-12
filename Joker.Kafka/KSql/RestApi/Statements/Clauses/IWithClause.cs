using Kafka.DotNet.ksqlDB.KSql.Query.Context;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Clauses
{
  public interface IWithClause
  {
    IAsClause With(CreationMetadata creationMetadata);
  }
}