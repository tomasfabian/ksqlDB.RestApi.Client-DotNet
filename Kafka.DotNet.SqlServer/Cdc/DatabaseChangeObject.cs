namespace Kafka.DotNet.SqlServer.Cdc
{
  public class DatabaseChangeObject
  {
    public string Before { get; set; }
    public string After { get; set; }
    public string Source { get; set; }
    
    public string Op { get; set; }
  }
}