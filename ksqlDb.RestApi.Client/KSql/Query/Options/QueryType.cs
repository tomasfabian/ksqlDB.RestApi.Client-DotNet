namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Specifies KSQL query endpoints when using the ksqlDB REST API.
/// </summary>
public enum QueryType
{
  /// <summary>
  /// Represents the "/ksql" endpoint in the ksqlDB REST API.
  /// </summary>
  Query,

  /// <summary>
  /// Represents the "/query-stream" endpoint in the ksqlDB REST API.
  /// </summary>
  QueryStream
}
