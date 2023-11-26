using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDb.RestApi.Client.Tests.Models;

public class Tweet : Record
{
  public int Id { get; set; }

  [JsonPropertyName("MESSAGE")]
  public string Message { get; set; } = null!;

  public bool IsRobot { get; set; }

  public double Amount { get; set; }

  public decimal AccountBalance { get; set; }
}
