using Kafka.DotNet.ksqlDB.KSql.Query;

namespace ksqlDB.Api.Client.Samples.Models.Movies
{
  public class Movie : Record
  {
    public string Title { get; set; }
    public int Id { get; set; }
    public int Release_Year { get; set; }
  }
}