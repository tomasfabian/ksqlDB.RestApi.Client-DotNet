using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Tests.Pocos
{
  public class Tweet : Record
  {
    public int Id { get; set; }

    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; }
    
    public bool IsRobot { get; set; }

    public double Amount { get; set; }

    public decimal AccountBalance { get; set; }
  }
}