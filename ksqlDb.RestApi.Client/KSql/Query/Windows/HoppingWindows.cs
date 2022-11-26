using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

public class HoppingWindows : TimeWindows
{
  public HoppingWindows(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit) 
    : base(duration, outputRefinement)
  {
    AdvanceBy = duration;
  }    

  public Duration AdvanceBy { get; private set; }

  public HoppingWindows WithAdvanceBy(Duration advanceBy)
  {
    AdvanceBy = advanceBy ?? throw new ArgumentNullException(nameof(advanceBy));

    if (AdvanceBy.TotalSeconds.Value > Duration.TotalSeconds.Value)
      throw new InvalidOperationException("Window advancement interval should be more than zero and less than window duration");

    return this;
  }

  public Duration Retention { get; private set; }

  public HoppingWindows WithRetention(Duration retention)
  {
    Retention = retention ?? throw new ArgumentNullException(nameof(retention));

    return this;
  }
}