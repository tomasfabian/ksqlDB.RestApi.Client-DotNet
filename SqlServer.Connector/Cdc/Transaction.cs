using System.Text.Json.Serialization;

namespace SqlServer.Connector.Cdc
{
  public record Transaction
  {
    public string Id { get; set; }

    [JsonPropertyName("total_order")]
    public string TotalOrder { get; set; }

    [JsonPropertyName("data_collection_order")]
    public string DataCollectionOrder { get; set; }
  }
}