using System;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.DotNet.ksqlDB.KSql.Query.PullQueries;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries
{
  public static class PullQueryExtensions
  {
    #region Where

    private static MethodInfo whereTSource;

    private static MethodInfo WhereTSource(Type TSource) =>
      (whereTSource ??= new Func<IPullable<object>, Expression<Func<object, bool>>, IPullable<object>>(Where).GetMethodInfo().GetGenericMethodDefinition())
      .MakeGenericMethod(TSource);

    public static IPullable<TSource> Where<TSource>(this IPullable<TSource> source, Expression<Func<TSource, bool>> predicate)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));

      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));

      return source.Provider.CreateQuery<TSource>(
        Expression.Call(
          null,
          WhereTSource(typeof(TSource)),
          source.Expression, Expression.Quote(predicate)
        ));
    }

    #endregion

    #region ToQueryString

    public static string ToQueryString<TSource>(this IPullable<TSource> source)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));

      var pullSet = source as KPullSet<TSource>;

      var dependencies = pullSet?.GetDependencies();

      return dependencies?.QueryStreamParameters.Sql;
    }

    #endregion
  }
}