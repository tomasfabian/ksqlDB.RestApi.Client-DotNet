using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public sealed class QueryStreamParameters : QueryStreamEndpointParameters<QueryStreamParameters>, IPushQueryParameters
{
  public static readonly string AutoOffsetResetPropertyName = "auto.offset.reset";

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
