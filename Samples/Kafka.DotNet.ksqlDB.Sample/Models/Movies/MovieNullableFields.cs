using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.Api.Client.Samples.Models.Movies
{
  public class MovieNullableFields
  {
    public string Title { get; set; }    
    [Key]
    public int? Id { get; set; }
    public int? Release_Year { get; set; }
  }
}