using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  public sealed class KSqlDbStatement : QueryParameters
  {
    public KSqlDbStatement(string statement)
    {
      if (string.IsNullOrEmpty(statement))
        throw new NullReferenceException(nameof(statement));

      Sql = statement;

      EndpointType = EndpointType.KSql;
    }

    [JsonIgnore]
    public Encoding ContentEncoding { get; set; } = Encoding.UTF8;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("commandSequenceNumber")]
    public long? CommandSequenceNumber  { get; set; }
  }
}