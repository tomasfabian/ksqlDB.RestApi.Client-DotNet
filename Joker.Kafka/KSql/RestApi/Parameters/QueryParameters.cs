using System.Collections.Generic;
using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  public class QueryParameters : IQueryParameters
  {
    [JsonPropertyName("ksql")]
    public string Sql { get; set; }

    [JsonPropertyName("streamsProperties")]
    public Dictionary<string, string> Properties { get; } = new();

    public static readonly string AutoOffsetResetPropertyName = "ksql.streams.auto.offset.reset";

    public string this[string key]
    {
      get => Properties[key];
      set => Properties[key] = value;
    }

    internal EndpointType EndpointType { get; set; } = EndpointType.Query;
  }
}