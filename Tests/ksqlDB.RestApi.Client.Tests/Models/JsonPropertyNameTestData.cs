using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDb.RestApi.Client.Tests.Models
{
  public class JsonPropertyNameTestData : Record
  {
    [JsonPropertyName("sub")] public SubProperty SubProperty { get; set; } = null!;
  }

  public class SubProperty
  {
    [JsonPropertyName("prop")] public string Property { get; set; } = null!;
  }
}
