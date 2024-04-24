namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Specifies KSQL query endpoints when using the ksqlDB REST API.
/// </summary>
public enum EndpointType
{
  /// <summary>
  /// Represents the "/query" endpoint in the ksqlDB REST API.
  /// </summary>
  Query = 0,

#if !NETSTANDARD
  /// <summary>
  /// Represents the "/query-stream" endpoint in the ksqlDB REST API.
  /// </summary>
  QueryStream = 1
#endif
}
