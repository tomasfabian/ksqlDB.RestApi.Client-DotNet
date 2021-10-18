namespace ksqlDB.Api.Client.Tests.Models.Movies
{
  public class Movie
  { 
    [Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations.IgnoreByInserts]
    public long RowTime { get; set; }
    public string Title { get; set; }
    [Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations.Key]
    public int Id { get; set; }
    public int Release_Year { get; set; }

    [Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations.IgnoreByInserts]
    public int IgnoreMe { get; set; }
  }
}