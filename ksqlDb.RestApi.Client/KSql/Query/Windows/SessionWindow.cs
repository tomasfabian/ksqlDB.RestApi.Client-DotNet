using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

public class SessionWindow : TimeWindows
{
  public SessionWindow(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit) 
    : base(duration, outputRefinement)
  {
  }
}