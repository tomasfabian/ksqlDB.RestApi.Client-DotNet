namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public interface IQueryOptions
{
  /// <summary>
  /// Property overrides serialized using each value's JSON type.
  /// </summary>
  Dictionary<string, object> Properties { get; }
}
