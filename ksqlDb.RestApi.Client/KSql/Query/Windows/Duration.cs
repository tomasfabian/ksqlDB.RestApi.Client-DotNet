namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

/// <summary>
/// Represents a duration of time.
/// </summary>
public class Duration
{
  /// <summary>
  /// Gets the time unit of the duration.
  /// </summary>
  public TimeUnits TimeUnit { get; private set; }

  /// <summary>
  /// Gets the value of the duration.
  /// </summary>
  public uint Value { get; private set; }

  /// <summary>
  /// Creates a new <see cref="Duration"/> object with the specified value in milliseconds.
  /// </summary>
  /// <param name="milliseconds">The value in milliseconds.</param>
  /// <returns>A new <see cref="Duration"/> object.</returns>
  public static Duration OfMilliseconds(uint milliseconds)
  {
    return new()
    {
      TimeUnit = TimeUnits.MILLISECONDS,
      Value = milliseconds
    };
  }

  /// <summary>
  /// Creates a new <see cref="Duration"/> object with the specified value in seconds.
  /// </summary>
  /// <param name="seconds">The value in seconds.</param>
  /// <returns>A new <see cref="Duration"/> object.</returns>
  public static Duration OfSeconds(uint seconds)
  {
    return new()
    {
      TimeUnit = TimeUnits.SECONDS,
      Value = seconds
    };
  }

  /// <summary>
  /// Creates a new <see cref="Duration"/> object with the specified value in minutes.
  /// </summary>
  /// <param name="minutes">The value in minutes.</param>
  /// <returns>A new <see cref="Duration"/> object.</returns>
  public static Duration OfMinutes(uint minutes)
  {
    return new()
    {
      TimeUnit = TimeUnits.MINUTES,
      Value = minutes
    };
  }

  /// <summary>
  /// Creates a new <see cref="Duration"/> object with the specified value in hours.
  /// </summary>
  /// <param name="hours">The value in hours.</param>
  /// <returns>A new <see cref="Duration"/> object.</returns>
  public static Duration OfHours(uint hours)
  {
    return new()
    {
      TimeUnit = TimeUnits.HOURS,
      Value = hours
    };
  }

  /// <summary>
  /// Creates a new <see cref="Duration"/> object with the specified value in days.
  /// </summary>
  /// <param name="days">The value in days.</param>
  /// <returns>A new <see cref="Duration"/> object.</returns>
  public static Duration OfDays(uint days)
  {
    return new()
    {
      TimeUnit = TimeUnits.DAYS,
      Value = days
    };
  }

  /// <summary>
  /// Gets the total duration in seconds.
  /// </summary>
  public Duration TotalSeconds
  {
    get
    {
      var totalSeconds = TimeUnit switch
      {
        TimeUnits.SECONDS => Value,
        TimeUnits.MINUTES => Value * 60,
        TimeUnits.HOURS => Value * 60 * 60,
        TimeUnits.DAYS => Value * 60 * 60 * 24,
        _ => throw new ArgumentOutOfRangeException()
      };

      return OfSeconds(totalSeconds);
    }
  }
}
