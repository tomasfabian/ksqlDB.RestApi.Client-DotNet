namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents parameters for a KSqlDb endpoint.
/// </summary>
public interface IKSqlDbParameters : IQueryParameters
{
  /// <summary>
  /// Clones the current parameters.
  /// </summary>
  /// <returns>A new instance of the parameters with the same values.</returns>
  IKSqlDbParameters Clone();
}
