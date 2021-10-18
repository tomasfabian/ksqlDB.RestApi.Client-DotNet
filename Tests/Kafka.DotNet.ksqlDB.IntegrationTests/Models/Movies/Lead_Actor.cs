namespace ksqlDB.Api.Client.IntegrationTests.Models.Movies
{
  public record Lead_Actor : Record
  {
    public string Title { get; set; }
    public string Actor_Name { get; set; }
  }
}