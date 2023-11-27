namespace ksqlDB.RestApi.Client.Samples.Models.InvocationFunctions;

internal record Lambda
{
  public int Id { get; set; }

  public int[] Lambda_Arr { get; set; } = null!;
  // public IEnumerable<int> Lambda_Arr { get; set; }

  public IDictionary<string, int[]> DictionaryArrayValues { get; set; } = null!;
  public IDictionary<string, int> DictionaryInValues { get; set; } = null!;
}