namespace ksqlDB.Api.Client.Samples.Models;

public class Click
{
  public string? IP_ADDRESS { get; set; }
  public string URL { get; set; } = null!;
  public string TIMESTAMP { get; set; } = null!;
}
