using System.Linq.Expressions;
using System.Reflection;
using ksqlDB.RestApi.Client.KSql.Query.PullQueries;

namespace ksqlDB.RestApi.Client.KSql.Linq.PullQueries;

public static class PullQueryExtensions
{
  #region Select

  private static MethodInfo? selectTSourceTResult;

  private static MethodInfo SelectTSourceTResult(Type source, Type result) =>
    (selectTSourceTResult ??= new Func<IPullable<object>, Expression<Func<object, object>>, IPullable<object>>(Select).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, result);

  /// <summary>
  /// Projects a single pull query response into a new form.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source.</typeparam>
  /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
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

  private static MethodInfo? whereTSource;

  private static MethodInfo WhereTSource(Type source) =>
    (whereTSource ??= new Func<IPullable<object>, Expression<Func<object, bool>>, IPullable<object>>(Where).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source);

  /// <summary>
  /// The WHERE clause must contain a value for each primary-key column to retrieve and may optionally include bounds on WINDOWSTART and WINDOWEND if the materialized table is windowed.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
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

  #region Take

  private static MethodInfo? takeTSource;

  private static MethodInfo TakeTSource(Type source) =>
    (takeTSource ??= new Func<IPullable<object>, int, IPullable<object>>(Take).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source);

  /// <summary>
  /// Restrict the number of rows returned by executing a pull query over a STREAM or a TABLE. ksqldb 0.24.0
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="count">The number of elements to return.</param>
  /// <returns>An observable sequence that contains the specified number of elements from the start of the input sequence.</returns>
  public static IPullable<TSource> Take<TSource>(this IPullable<TSource> source, int count)
  {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        TakeTSource(typeof(TSource)),
        source.Expression, Expression.Constant(count)
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

    var pullSet = (KPullSet<TSource>)source;

    var dependencies = pullSet.GetDependencies();

    return pullSet.GetQueryStreamParameters(dependencies).Sql;
  }

  #endregion
}
