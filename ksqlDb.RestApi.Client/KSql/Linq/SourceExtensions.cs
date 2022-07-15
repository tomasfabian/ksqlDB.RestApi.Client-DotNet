using System;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDB.RestApi.Client.KSql.Linq;

internal static class SourceExtensions
{
  public static ISource<TSource> Within<TSource>(this ISource<TSource> source, Duration duration)
  {
    if(source is Source<TSource> s)
      s.DurationBefore = duration;

    return source;
  }

  public static ISource<TSource> Within<TSource>(this ISource<TSource> source, Duration durationBefore, Duration durationAfter)
  {
    if (source is Source<TSource> s)
    {
      s.DurationBefore = durationBefore;
      s.DurationAfter = durationAfter;
    }

    return source;
  }
}