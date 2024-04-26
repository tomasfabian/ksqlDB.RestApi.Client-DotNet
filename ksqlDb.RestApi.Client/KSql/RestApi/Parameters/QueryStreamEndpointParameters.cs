using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters
{
  public class QueryStreamEndpointParameters<T> : IKSqlDbParameters
    where T : QueryStreamEndpointParameters<T>, new()
  {
    [JsonPropertyName("sql")]
    public string Sql { get; set; } = null!;

    [JsonPropertyName("properties")]
    public Dictionary<string, string> Properties { get; } = new();

    public string this[string key]
    {
      get => Properties[key];
      set => Properties[key] = value;
    }

    public void FillFrom(IKSqlDbParameters parameters)
    {
      this.FillQueryParametersFrom(parameters);
    }

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

    public override string ToString()
    {
      return this.ToLogInfo();
    }
  }
}
