using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  public class QueryParameters : IKSqlDbParameters
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
    
    [JsonIgnore]
    public AutoOffsetReset AutoOffsetReset
    {
      get
      {
        var value = this[AutoOffsetResetPropertyName];

        if (value == "earliest")
          return AutoOffsetReset.Earliest;
        
        return AutoOffsetReset.Latest;
      }

      set => this[AutoOffsetResetPropertyName] = value.ToString().ToLower();
    }

    internal EndpointType EndpointType { get; set; } = EndpointType.Query;

    public IKSqlDbParameters Clone()
    {
      var queryParams = new QueryParameters()
      {
        Sql = Sql,
        EndpointType = EndpointType
      };

      foreach (var entry in Properties)
        queryParams.Properties.Add(entry.Key, entry.Value);

      return queryParams;
    }
  }
}