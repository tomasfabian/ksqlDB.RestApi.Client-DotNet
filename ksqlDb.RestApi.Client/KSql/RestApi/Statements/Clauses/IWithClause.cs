namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;

public interface IWithClause
{
  IAsClause With(CreationMetadata creationMetadata);
}