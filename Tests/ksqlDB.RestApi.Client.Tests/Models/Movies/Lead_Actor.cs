using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDb.RestApi.Client.Tests.Models.Movies;

public class Lead_Actor : Record
{
  public string Title { get; set; } = null!;
  public string Actor_Name { get; set; } = null!;
}
