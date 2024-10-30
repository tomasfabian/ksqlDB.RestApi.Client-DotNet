using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents parameters for a query stream.
/// </summary>
public sealed class QueryStreamParameters : QueryStreamEndpointParameters<QueryStreamParameters>, IPushQueryParameters
{
  public static readonly string AutoOffsetResetPropertyName = "auto.offset.reset";

  /// <summary>
  /// Sets the auto offset reset using <see cref="AutoOffsetReset"/>.
  /// </summary>
  [JsonIgnore]
  public AutoOffsetReset AutoOffsetReset
  {
    get => Get<AutoOffsetReset>(AutoOffsetResetPropertyName);

    set => Set(AutoOffsetResetPropertyName, value);
  }
}
