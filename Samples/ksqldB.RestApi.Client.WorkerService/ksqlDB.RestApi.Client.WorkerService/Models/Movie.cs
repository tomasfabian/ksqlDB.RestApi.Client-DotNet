using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.RestApi.Client.WorkerService.Models;

public record Movie
{
  [IgnoreByInserts]
  public long RowTime { get; set; }

  public string Title { get; set; }
  public int Id { get; set; }
  public int Release_Year { get; set; }
}