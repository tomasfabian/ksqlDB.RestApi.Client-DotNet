using static System.String;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  internal static class StatementTemplates
  {
    public static string ShowStreams => "SHOW STREAMS;";
    public static string ShowTables => "SHOW TABLES;";
    public static string ShowQueries => "SHOW QUERIES;";

    public static string ShowTopics => "SHOW TOPICS;";
    public static string ShowAllTopics => "SHOW ALL TOPICS;";
    public static string ShowTopicsExtended => "SHOW TOPICS EXTENDED;";
    public static string ShowAllTopicsExtended => "SHOW ALL TOPICS EXTENDED;";

    public static string ShowConnectors => "SHOW CONNECTORS;";
    public static string DropConnector(string connectorName) => $"DROP CONNECTOR {connectorName};";

    public static string DropStream(string streamName, bool useIfExists = false, bool deleteTopic = false)
    {
      string ifExistsClause = useIfExists ? " IF EXISTS" : Empty;
      string deleteTopicClause = deleteTopic ? " DELETE TOPIC" : Empty;

      return $"DROP STREAM{ifExistsClause} {streamName}{deleteTopicClause};";
    } 

    public static string DropTable(string tableName, bool useIfExists = false, bool deleteTopic = false)
    {
      string ifExistsClause = useIfExists ? " IF EXISTS" : Empty;
      string deleteTopicClause = deleteTopic ? " DELETE TOPIC" : Empty;
      
      return $"DROP TABLE{ifExistsClause} {tableName}{deleteTopicClause};";
    }

    public static string TerminatePersistentQuery(string queryId) => $"TERMINATE {queryId};";

    public static string Explain(string sqlExpression) => $"EXPLAIN {sqlExpression}";

    public static string ExplainBy(string queryId) => $"{Explain(queryId)};";
  }
}