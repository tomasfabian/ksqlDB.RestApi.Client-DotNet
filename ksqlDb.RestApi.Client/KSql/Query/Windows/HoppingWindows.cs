using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

#nullable enable
/// <summary>
/// Represents hopping windows for time-based operations.
/// </summary>
public class HoppingWindows : TimeWindows
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HoppingWindows"/> class with the specified duration and output refinement.
  /// </summary>
  /// <param name="duration">The duration of the hopping window.</param>
  /// <param name="outputRefinement">The output refinement mode.</param>
  public HoppingWindows(Duration duration, OutputRefinement outputRefinement = OutputRefinement.Emit) 
    : base(duration, outputRefinement)
  {
    AdvanceBy = duration;
  }

  /// <summary>
  /// Gets or sets the interval by which the window advances.
  /// </summary>
  public Duration AdvanceBy { get; private set; }

  /// <summary>
  /// Sets the advancement interval for the hopping window.
  /// </summary>
  /// <param name="advanceBy">The interval by which the window advances.</param>
  /// <returns>The updated <see cref="HoppingWindows"/> object.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="advanceBy"/> is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the window advancement interval is greater than the window duration.</exception>
  public HoppingWindows WithAdvanceBy(Duration advanceBy)
  {
    AdvanceBy = advanceBy ?? throw new ArgumentNullException(nameof(advanceBy));

    if (AdvanceBy.TotalSeconds.Value > Duration.TotalSeconds.Value)
      throw new InvalidOperationException("Window advancement interval should be more than zero and less than window duration");

    return this;
  }

  /// <summary>
  /// Gets the retention duration for the hopping window.
  /// </summary>
  public Duration? Retention { get; private set; }

  /// <summary>
  /// Sets the retention duration for the hopping window.
  /// </summary>
  /// <param name="retention">The retention duration.</param>
  /// <returns>The updated <see cref="HoppingWindows"/> object.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="retention"/> is null.</exception>
  public HoppingWindows WithRetention(Duration retention)
  {
    Retention = retention ?? throw new ArgumentNullException(nameof(retention));

    return this;
  }
}
