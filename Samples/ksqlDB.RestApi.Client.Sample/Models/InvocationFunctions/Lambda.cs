using System.Collections.Generic;

namespace ksqlDB.Api.Client.Samples.Models.InvocationFunctions
{
  record Lambda
  {
    public int Id { get; set; }
    public int[] Lambda_Arr { get; set; }
    // public IEnumerable<int> Lambda_Arr { get; set; }

    public IDictionary<string, int[]> DictionaryArrayValues { get; set; }
    public IDictionary<string, int> DictionaryInValues { get; set; }
  }
}