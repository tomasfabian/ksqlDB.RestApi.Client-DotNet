using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDb.RestApi.Client.Tests.Models
{
  public class JsonPropertyNameTestData : Record
  {
    [JsonPropertyName("sub")] public SubProperty SubProperty { get; set; }
  }

  [Struct]
  public class SubProperty
  {
    [JsonPropertyName("prop")] public string Property { get; set; }
  }
}
