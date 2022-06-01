using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using static System.String;

namespace ksqlDB.RestApi.Client.WorkerService.Models;

public record Movie
{
  [IgnoreByInserts]
  public long RowTime { get; set; }

  public string Title { get; set; } = Empty;
  public int Id { get; set; }
  public int Release_Year { get; set; }
}