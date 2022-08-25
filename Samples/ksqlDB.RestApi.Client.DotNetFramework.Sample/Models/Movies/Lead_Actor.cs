using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.RestApi.Client.DotNetFramework.Sample.Models.Movies
{
  public class Lead_Actor : Record
  {
    public string Title { get; set; } = null!;
    public string Actor_Name { get; set; } = null!;
  }
}