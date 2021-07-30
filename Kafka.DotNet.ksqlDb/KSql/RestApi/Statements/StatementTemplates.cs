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

    public static string TerminatePushQuery(string queryId) => $"TERMINATE {queryId};";
  }
}