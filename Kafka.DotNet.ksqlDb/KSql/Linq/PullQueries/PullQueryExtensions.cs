using System;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.DotNet.ksqlDB.KSql.Query.PullQueries;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries
{
  public static class PullQueryExtensions
  {
    #region Select

    private static MethodInfo selectTSourceTResult;

    private static MethodInfo SelectTSourceTResult(Type TSource, Type TResult) =>
      (selectTSourceTResult ??= new Func<IPullable<object>, Expression<Func<object, object>>, IPullable<object>>(Select).GetMethodInfo().GetGenericMethodDefinition())
      .MakeGenericMethod(TSource, TResult);

    /// <summary>
    /// Projects a single pull query response into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
    /// <param name="source"></param>
    /// <param name="selector">A transform function to apply to the single response.</param>
    /// <returns></returns>
    public static IPullable<TResult> Select<TSource, TResult>(this IPullable<TSource> source, Expression<Func<TSource, TResult>> selector)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));

      if (selector == null)
        throw new ArgumentNullException(nameof(selector));

      return source.Provider.CreateQuery<TResult>(
        Expression.Call(
          null,
          SelectTSourceTResult(typeof(TSource), typeof(TResult)),
          source.Expression, Expression.Quote(selector)
        ));
    }

    #endregion

    #region Where

    private static MethodInfo whereTSource;

    private static MethodInfo WhereTSource(Type TSource) =>
      (whereTSource ??= new Func<IPullable<object>, Expression<Func<object, bool>>, IPullable<object>>(Where).GetMethodInfo().GetGenericMethodDefinition())
      .MakeGenericMethod(TSource);

    /// <summary>
    /// The WHERE clause must contain a value for each primary-key column to retrieve and may optionally include bounds on WINDOWSTART and WINDOWEND if the materialized table is windowed.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Generates the KSQL pull query.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
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