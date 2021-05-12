using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.Sample.Models.Movies
{
  public class MovieNullableFields
  {
    public string Title { get; set; }    
    [Key]
    public int? Id { get; set; }
    public int? Release_Year { get; set; }
  }
}