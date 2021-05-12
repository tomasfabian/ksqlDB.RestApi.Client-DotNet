namespace Kafka.DotNet.ksqlDB.KSql.Query.Windows
{
  public class SessionWindow : TimeWindows
  {
    public SessionWindow(Duration duration) 
      : base(duration)
    {
    }
  }
}