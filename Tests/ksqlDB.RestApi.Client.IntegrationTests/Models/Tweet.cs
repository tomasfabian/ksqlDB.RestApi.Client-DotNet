using System.Text.Json.Serialization;

namespace ksqlDb.RestApi.Client.IntegrationTests.Models;

public record Tweet : Record
{
  public int Id { get; set; }

  [JsonPropertyName("MESSAGE")]
  public string Message { get; set; } = null!;

  public bool IsRobot { get; set; }

  public double Amount { get; set; }

  public decimal AccountBalance { get; set; }
}
