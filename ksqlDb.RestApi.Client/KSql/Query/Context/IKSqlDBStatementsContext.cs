using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public interface IKSqlDBStatementsContext
  {
    /// <summary>
    /// Create a new materialized stream view, along with the corresponding Kafka topic, and stream the result of the query into the topic.
    /// </summary>
    /// <param name="streamName">Name of the stream to create.</param>
    /// <returns></returns>
    IWithOrAsClause CreateStreamStatement(string streamName);

    /// <summary>
    /// Create or replace a materialized stream view, along with the corresponding Kafka topic, and stream the result of the query into the topic.
    /// </summary>
    /// <param name="streamName">Name of the stream to create or replace.</param>
    /// <returns></returns>
    IWithOrAsClause CreateOrReplaceStreamStatement(string streamName);

    /// <summary>
    /// Create a new ksqlDB materialized table view, along with the corresponding Kafka topic, and stream the result of the query as a changelog into the topic.
    /// </summary>
    /// <param name="tableName">Name of the table to create.</param>
    /// <returns></returns>
    IWithOrAsClause CreateTableStatement(string tableName);

    /// <summary>
    /// Create or replace a ksqlDB materialized table view, along with the corresponding Kafka topic, and stream the result of the query as a changelog into the topic.
    /// </summary>
    /// <param name="tableName">Name of the table to create or replace.</param>
    /// <returns></returns>
    IWithOrAsClause CreateOrReplaceTableStatement(string tableName);
  }
}