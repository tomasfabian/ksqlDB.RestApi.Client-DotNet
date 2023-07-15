using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

/// <summary>
/// Represents session windows for time-based operations.
/// </summary>
public class SessionWindow : TimeWindows
{
  /// <summary>
  /// Initializes a new instance of the <see cref="SessionWindow"/> class with the specified duration and output refinement.
  /// </summary>
  /// <param name="duration">The duration of the session window.</param>
  /// <param name="outputRefinement">The output refinement mode.</param>
  public SessionWindow(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit) 
    : base(duration, outputRefinement)
  {
  }
}
