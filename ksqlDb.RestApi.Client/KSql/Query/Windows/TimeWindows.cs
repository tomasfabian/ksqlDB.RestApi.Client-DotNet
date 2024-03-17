using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

/// <summary>
/// Represents time windows for time-based operations.
/// </summary>
public class TimeWindows
{
  /// <summary>
  /// Initializes a new instance of the <see cref="TimeWindows"/> class with the specified duration and output refinement.
  /// </summary>
  /// <param name="duration">The duration of the time window.</param>
  /// <param name="outputRefinement">The output refinement mode.</param>
  public TimeWindows(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit)
  {
    Duration = duration ?? throw new ArgumentNullException(nameof(duration));

    OutputRefinement = outputRefinement;
  }

  /// <summary>
  /// Gets the duration of the time window.
  /// </summary>
  public Duration Duration { get; }

  /// <summary>
  /// Gets the grace period for the time window.
  /// </summary>
  public Duration? GracePeriod { get; private set; }

  /// <summary>
  /// Gets the output refinement mode of the time window.
  /// </summary>
  public OutputRefinement OutputRefinement { get; }

  /// <summary>
  /// Sets the grace period for the time window.
  /// </summary>
  /// <param name="gracePeriod">The grace period duration.</param>
  /// <returns>The updated <see cref="TimeWindows"/> object.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="gracePeriod"/> is null.</exception>
  public TimeWindows WithGracePeriod(Duration gracePeriod)
  {
    GracePeriod = gracePeriod ?? throw new ArgumentNullException(nameof(gracePeriod));

    return this;
  }
}
