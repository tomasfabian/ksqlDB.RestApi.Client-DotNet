using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Sample.Models.Movies
{
  public class Movie : Record
  {
    public string Title { get; set; }
    public int Id { get; set; }
    public int Release_Year { get; set; }
  }
}