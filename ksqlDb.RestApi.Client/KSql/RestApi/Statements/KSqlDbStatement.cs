using System;
using System.Text;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

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