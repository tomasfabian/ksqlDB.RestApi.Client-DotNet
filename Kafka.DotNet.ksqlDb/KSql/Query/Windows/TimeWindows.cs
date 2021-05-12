using System;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Windows
{
  public class TimeWindows
  {
    public Duration Duration { get; }

    public TimeWindows(Duration duration)
    {
      Duration = duration ?? throw new ArgumentNullException(nameof(duration));
    }
    
    public Duration GracePeriod { get; private set; }

    public TimeWindows WithGracePeriod(Duration gracePeriod)
    {
      GracePeriod = gracePeriod ?? throw new ArgumentNullException(nameof(gracePeriod));

      return this;
    }
  }
}