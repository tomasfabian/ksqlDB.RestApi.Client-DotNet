using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.Api.Client.Samples.Models.Movies
{
  public class Lead_Actor : Record
  {
    public string Title { get; set; }
    public string Actor_Name { get; set; }
  }
}