using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

public class TimeWindows
{
  public TimeWindows(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit)
  {
    Duration = duration ?? throw new ArgumentNullException(nameof(duration));

    OutputRefinement = outputRefinement;
  }

  public Duration Duration { get; }
  public Duration GracePeriod { get; private set; }
  public OutputRefinement OutputRefinement { get; }

  public TimeWindows WithGracePeriod(Duration gracePeriod)
  {
    GracePeriod = gracePeriod ?? throw new ArgumentNullException(nameof(gracePeriod));

    return this;
  }
}