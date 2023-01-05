namespace ksqlDB.Api.Client.IntegrationTests.Models.Movies;

public record Lead_Actor : Record
{
  public string Title { get; set; } = null!;
  public string Actor_Name { get; set; } = null!;
}
