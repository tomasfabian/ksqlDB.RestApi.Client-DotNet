using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.KSql.Query
{
  public class Record
  {
    [IgnoreByInserts]
    public long RowTime { get; set; }
  }
}