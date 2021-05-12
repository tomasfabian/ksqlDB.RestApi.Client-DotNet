using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Sample.Models.Movies
{
  public class Lead_Actor : Record
  {
    public string Title { get; set; }
    public string Actor_Name { get; set; }
  }
}