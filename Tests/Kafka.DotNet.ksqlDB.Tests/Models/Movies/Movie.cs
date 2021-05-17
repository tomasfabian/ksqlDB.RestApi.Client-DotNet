using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Tests.Models.Movies
{
  public class Movie
  { 
    [KSql.RestApi.Statements.Annotations.IgnoreByInserts]
    public long RowTime { get; set; }
    public string Title { get; set; }
    [KSql.RestApi.Statements.Annotations.Key]
    public int Id { get; set; }
    public int Release_Year { get; set; }

    [KSql.RestApi.Statements.Annotations.IgnoreByInserts]
    public int IgnoreMe { get; set; }
  }
}