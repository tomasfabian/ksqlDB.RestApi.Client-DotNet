using System;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDB.RestApi.Client.KSql.Linq
{
  public static class SourceExtensions
  {
    public static ISource<TSource> Within<TSource>(this ISource<TSource> source, Duration duration)
    {
      if(source is Source<TSource> s)
        s.Duration = duration;

      return source;
    }
  }
}