namespace Kafka.DotNet.ksqlDB.Sample.Models.InvocationFunctions
{
  record Lambda
  {
    public int Id { get; set; }
    public int[] Lambda_Arr { get; set; }
    // public IEnumerable<int> Lambda_Arr { get; set; }
  }
}