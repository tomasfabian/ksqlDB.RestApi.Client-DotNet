using ksqlDB.RestApi.Client.KSql.Query;

namespace ksqlDB.Api.Client.Tests.Models;

internal class Transaction : Record
{
  public string CardNumber { get; set; } = null!;
  public decimal Amount { get; set; }
  public int[] Array { get; set; } = null!;
  public IDictionary<string, int> Dictionary { get; set; } = null!;
}
