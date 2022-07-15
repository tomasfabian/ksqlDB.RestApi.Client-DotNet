namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

public class SessionWindow : TimeWindows
{
  public SessionWindow(Duration duration) 
    : base(duration)
  {
  }
}