using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents parameters for a query.
/// </summary>
public class QueryParameters : QueryEndpointParameters<QueryParameters>, IPushQueryParameters
{
  public static readonly string AutoOffsetResetPropertyName = "ksql.streams.auto.offset.reset";

  /// <summary>
  /// Sets the auto offset reset using <see cref="AutoOffsetReset"/>.
  /// </summary>
  [JsonIgnore]
  public AutoOffsetReset AutoOffsetReset
  {
    get
    {
      var value = this[AutoOffsetResetPropertyName];

      return value.ToAutoOffsetReset();
    }

    set => this[AutoOffsetResetPropertyName] = value.ToKSqlValue();
  }
}
