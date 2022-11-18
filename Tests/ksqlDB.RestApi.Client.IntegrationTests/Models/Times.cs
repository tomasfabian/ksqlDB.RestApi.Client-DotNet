namespace ksqlDB.Api.Client.IntegrationTests.Models;

public record Times : Record
{
  public int Id { get; set; }

  public DateTime Created { get; set; }

  public DateTimeOffset CreatedWithOffset { get; set; }

  public TimeSpan CreatedTime { get; set; }
}