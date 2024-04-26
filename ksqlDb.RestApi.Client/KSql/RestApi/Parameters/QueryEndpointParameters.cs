using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters
{
  public class QueryEndpointParameters<T> : IKSqlDbParameters
    where T : QueryEndpointParameters<T>, new()
  {
    /// <summary>
    /// A semicolon-delimited sequence of SQL statements to run.
    /// </summary>
    [JsonPropertyName("ksql")]
    public string Sql { get; set; } = null!;

    /// <summary>
    /// Property overrides to run the statements with.
    /// </summary>
    [JsonPropertyName("streamsProperties")]
    public Dictionary<string, string> Properties { get; } = new();

    public string this[string key]
    {
      get => Properties[key];
      set => Properties[key] = value;
    }

    internal EndpointType EndpointType { get; set; } = EndpointType.Query;

    public void FillFrom(IKSqlDbParameters parameters)
    {
      this.FillFromInternal(parameters);
    }

    public IKSqlDbParameters Clone()
    {
      var queryParams = new T()
      {
        Sql = Sql,
        EndpointType = EndpointType
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
