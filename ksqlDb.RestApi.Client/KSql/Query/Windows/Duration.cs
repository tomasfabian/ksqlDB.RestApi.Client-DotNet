using System;

namespace ksqlDB.RestApi.Client.KSql.Query.Windows;

public class Duration
{
  public TimeUnits TimeUnit { get; private set; }
  public uint Value { get; private set; }

  public static Duration OfMilliseconds(uint seconds)
  {
    return new()
    {
      TimeUnit = TimeUnits.MILLISECONDS,
      Value = seconds
    };
  }

  public static Duration OfSeconds(uint seconds)
  {
    return new()
    {
      TimeUnit = TimeUnits.SECONDS,
      Value = seconds
    };
  }

  public static Duration OfMinutes(uint minutes)
  {
    return new()
    {
      TimeUnit = TimeUnits.MINUTES,
      Value = minutes
    };
  }

  public static Duration OfHours(uint hours)
  {
    return new()
    {
      TimeUnit = TimeUnits.HOURS,
      Value = hours
    };
  }

  public static Duration OfDays(uint days)
  {
    return new()
    {
      TimeUnit = TimeUnits.DAYS,
      Value = days
    };
  }

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

      return Duration.OfSeconds(totalSeconds);
    }
  }
}