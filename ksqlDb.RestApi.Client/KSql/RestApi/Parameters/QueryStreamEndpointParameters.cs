using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents parameters for a '/query-stream' endpoint.
/// </summary>
/// <typeparam name="T">The type of the query stream endpoint parameters.</typeparam>
public class QueryStreamEndpointParameters<T> : IKSqlDbParameters
  where T : QueryStreamEndpointParameters<T>, new()
{
  /// <summary>
  /// A semicolon-delimited sequence of SQL statements to run.
  /// </summary>
  [JsonPropertyName("sql")]
  public string Sql { get; set; } = null!;

  /// <summary>
  /// Property overrides to run the statements with.
  /// </summary>
  [JsonPropertyName("properties")]
  public Dictionary<string, JsonValue> Properties { get; } = new();

  public TValue Get<TValue>(string key)
  {
    return Properties[key].GetValue<TValue>();
  }

  public void Set<TValue>(string key, TValue value)
  {
    Properties[key] = JsonValue.Create(value) ?? throw new InvalidOperationException("The value could not be converted to JSON");
  }

  /// <summary>
  /// Fills the parameters from another set of parameters.
  /// </summary>
  /// <param name="parameters">The parameters to fill from.</param>
  public void FillFrom(IKSqlDbParameters parameters)
  {
    this.FillQueryParametersFrom(parameters);
  }


  /// <summary>
  /// Clones the current parameters.
  /// </summary>
  /// <returns>A new instance of the parameters with the same values.</returns>
  public IKSqlDbParameters Clone()
  {
    var queryParams = new T()
    {
      Sql = Sql
    };

    foreach (var entry in Properties)
      queryParams.Properties.Add(entry.Key, entry.Value);

    return queryParams;
  }

  /// <summary>
  /// Returns a string representation of the object.
  /// </summary>
  /// <returns>A string representation of the object.</returns>
  public override string ToString()
  {
    return this.ToLogInfo();
  }
}
