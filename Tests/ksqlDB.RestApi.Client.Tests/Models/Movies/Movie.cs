using System.Collections.Generic;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.Api.Client.Tests.Models.Movies;

public class Movie
{ 
  [IgnoreByInserts]
  public long RowTime { get; set; }
  public string Title { get; set; }
  [Key]
  public int Id { get; set; }
  public int Release_Year { get; set; }

  [IgnoreByInserts]
  public int IgnoreMe { get; set; }

  public IEnumerable<int> ReadOnly { get; } = new[] { 1, 2 };
}