using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDb.RestApi.Client.Tests.Models.Movies;

public class Movie
{ 
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Ignore]
  public long RowTime { get; set; }
  public string Title { get; set; } = null!;
  [Key]
  public int Id { get; set; }
  public int Release_Year { get; set; }

  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Ignore]
  public int IgnoreMe { get; set; }

  public IEnumerable<int> ReadOnly { get; } = new[] { 1, 2 };
}
