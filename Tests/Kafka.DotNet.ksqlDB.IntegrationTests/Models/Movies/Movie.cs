namespace ksqlDB.Api.Client.IntegrationTests.Models.Movies
{
  public record Movie : Record
  {
    public string Title { get; set; }
    public int Id { get; set; }
    public int Release_Year { get; set; }
  }
}