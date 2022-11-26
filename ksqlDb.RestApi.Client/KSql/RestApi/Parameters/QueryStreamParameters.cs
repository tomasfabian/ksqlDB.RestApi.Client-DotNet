using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public sealed class QueryStreamParameters : IKSqlDbParameters
{
  [JsonPropertyName("sql")]
  public string Sql { get; set; }

  [JsonPropertyName("properties")]
  public Dictionary<string, string> Properties { get; } = new();
    
  public static readonly string AutoOffsetResetPropertyName = "auto.offset.reset";

  public string this[string key]
  {
    get => Properties[key];
    set => Properties[key] = value;
  }

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

  internal QueryType QueryType { get; } = QueryType.QueryStream;
    
  public IKSqlDbParameters Clone()
  {
    var queryParams = new QueryStreamParameters
    {
      Sql = Sql
    };

    foreach (var entry in Properties)
      queryParams.Properties.Add(entry.Key, entry.Value);

    return queryParams;
  }

  public override string ToString()
  {
    return this.ToLogInfo();
  }
}