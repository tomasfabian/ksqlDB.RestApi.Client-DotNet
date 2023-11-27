namespace ksqlDB.RestApi.Client.Samples.Models;

public record Address
{
  public int Number { get; set; }
  public string Street { get; set; } = null!;
}