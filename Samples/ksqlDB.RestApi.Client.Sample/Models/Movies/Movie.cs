using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.Samples.Models.Movies;

public class Movie : Record
{
  public int Id { get; set; }
  [Key]
  public string Title { get; set; } = null!;
  public int Release_Year { get; set; }
}
