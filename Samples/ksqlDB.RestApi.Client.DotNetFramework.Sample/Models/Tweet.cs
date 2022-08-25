using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.DotNetFramework.Sample.Models
{
  public class Tweet : Record
  {
    public int Id { get; set; }

    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; } = null!;


    public double Amount { get; set; }
    
    [Decimal(3,2)]
    public decimal AccountBalance { get; set; }
  }
}