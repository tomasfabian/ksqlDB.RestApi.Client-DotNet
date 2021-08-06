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

    public static string DropStream(string streamName) => $"DROP STREAM IF EXISTS {streamName} DELETE TOPIC;";
    //public static string DropStream(string streamName) => $"DROP STREAM [IF EXISTS] {streamName} [DELETE TOPIC];";
    public static string DropTable(string tableName) => $"DROP TABLE IF EXISTS {tableName};";
    public static string DropTableAndDeleteTopic(string tableName) => $"DROP TABLE IF EXISTS {tableName} DELETE TOPIC;";
    //public static string DropTable(string tableName) => $"DROP TABLE [IF EXISTS] {tableName} [DELETE TOPIC];";

    public static string TerminatePersistentQuery(string queryId) => $"TERMINATE {queryId};";
  }
}