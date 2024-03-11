using System.Linq.Expressions;
using System.Reflection;
using ksqlDB.RestApi.Client.KSql.Query.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDB.RestApi.Client.KSql.Linq.Statements;

#nullable enable
public static class CreateStatementExtensions
{
  #region Select

  private static MethodInfo? selectTSourceTResult;

  private static MethodInfo SelectTSourceTResult(Type source, Type result) =>
    (selectTSourceTResult ??= new Func<ICreateStatement<object>, Expression<Func<object, object>>, ICreateStatement<object>>(Select).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, result);

  public static ICreateStatement<TResult> Select<TSource, TResult>(this ICreateStatement<TSource> source, Expression<Func<TSource, TResult>> selector)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (selector == null)
      throw new ArgumentNullException(nameof(selector));

    return source.Provider.CreateStatement<TResult>(
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
    (whereTSource ??= new Func<ICreateStatement<object>, Expression<Func<object, bool>>, ICreateStatement<object>>(Where).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source);

  public static ICreateStatement<TSource> Where<TSource>(this ICreateStatement<TSource> source, Expression<Func<TSource, bool>> predicate)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return source.Provider.CreateStatement<TSource>(
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
    (takeTSource ??= new Func<ICreateStatement<object>, int, ICreateStatement<object>>(Take).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source);

  public static ICreateStatement<TSource> Take<TSource>(this ICreateStatement<TSource> source, int count)
  {
    return source.Provider.CreateStatement<TSource>(
      Expression.Call(
        null,
        TakeTSource(typeof(TSource)),
        source.Expression, Expression.Constant(count)
      ));
  }

  #endregion

  #region GroupBy

  private static MethodInfo? groupByTSourceTKey;

  private static MethodInfo GroupByTSourceTKey(Type source, Type key) =>
    (groupByTSourceTKey ??= new Func<ICreateStatement<object>, Expression<Func<object, object>>, ICreateStatement<IKSqlGrouping<object, object>>>(GroupBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, key);

  public static ICreateStatement<IKSqlGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this ICreateStatement<TSource> source, Expression<Func<TSource, TKey>> keySelector)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

    return source.Provider.CreateStatement<IKSqlGrouping<TKey, TSource>>(
      Expression.Call(
        null,
        GroupByTSourceTKey(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Quote(keySelector))
    );
  }

  #endregion

  #region Having

  private static MethodInfo? havingTSource;

  private static MethodInfo HavingTSource(Type source, Type key) =>
    (havingTSource ??= new Func<ICreateStatement<IKSqlGrouping<object, object>>, Expression<Func<IKSqlGrouping<object, object>, bool>>, ICreateStatement<IKSqlGrouping<object, object>>>(Having).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, key);

  public static ICreateStatement<IKSqlGrouping<TKey, TSource>> Having<TSource, TKey>(this ICreateStatement<IKSqlGrouping<TKey, TSource>> source, Expression<Func<IKSqlGrouping<TKey, TSource>, bool>> predicate)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return source.Provider.CreateStatement<IKSqlGrouping<TKey, TSource>>(
      Expression.Call(
        null,
        HavingTSource(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Quote(predicate)
      ));
  }

  #endregion

  #region WindowedBy

  private static MethodInfo? windowedByTSourceTKey;

  private static MethodInfo WindowedByTSourceTKey(Type source, Type key) =>
    (windowedByTSourceTKey ??= new Func<ICreateStatement<IKSqlGrouping<object, object>>, TimeWindows, ICreateStatement<IWindowedKSql<object, object>>>(WindowedBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, key);

  public static ICreateStatement<IWindowedKSql<TKey, TSource>> WindowedBy<TSource, TKey>(this ICreateStatement<IKSqlGrouping<TKey, TSource>> source, TimeWindows timeWindows)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (timeWindows == null) throw new ArgumentNullException(nameof(timeWindows));

    return source.Provider.CreateStatement<IWindowedKSql<TKey, TSource>>(
      Expression.Call(null,
        WindowedByTSourceTKey(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Constant(timeWindows)));
  }

  #endregion

  #region Join

  private static MethodInfo? joinTOuterTInnerTKeyTResult;

  private static MethodInfo JoinTOuterTInnerTKeyTResult(Type outer, Type inner, Type key, Type result) =>
    (joinTOuterTInnerTKeyTResult ??= new Func<ICreateStatement<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, ICreateStatement<object>>(Join).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(outer, inner, key, result);

  public static ICreateStatement<TResult> Join<TOuter, TInner, TKey, TResult>(this ICreateStatement<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateStatement<TResult>(
      Expression.Call(
        null,
        JoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region LeftJoin

  private static MethodInfo? leftJoinTOuterTInnerTKeyTResult;

  private static MethodInfo LeftJoinTOuterTInnerTKeyTResult(Type outer, Type inner, Type key, Type result) =>
    (leftJoinTOuterTInnerTKeyTResult ??= new Func<ICreateStatement<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, ICreateStatement<object>>(LeftJoin).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(outer, inner, key, result);

  public static ICreateStatement<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this ICreateStatement<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateStatement<TResult>(
      Expression.Call(
        null,
        LeftJoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region FullOuterJoin

  private static MethodInfo? fullOuterJoinTOuterTInnerTKeyTResult;

  private static MethodInfo FullOuterJoinTOuterTInnerTKeyTResult(Type outer, Type inner, Type key, Type result) =>
    (fullOuterJoinTOuterTInnerTKeyTResult ??= new Func<ICreateStatement<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, ICreateStatement<object>>(FullOuterJoin).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(outer, inner, key, result);

  public static ICreateStatement<TResult> FullOuterJoin<TOuter, TInner, TKey, TResult>(this ICreateStatement<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateStatement<TResult>(
      Expression.Call(
        null,
        FullOuterJoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region PartitionBy

  private static MethodInfo? partitionByTSourceTResult;

  private static MethodInfo PartitionByTSourceTResult(Type source, Type result) =>
    (partitionByTSourceTResult ??= new Func<ICreateStatement<object>, Expression<Func<object, object>>, ICreateStatement<object>>(PartitionBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, result);

  /// <summary>
  /// Repartition a stream.
  /// </summary>
  /// <typeparam name="TSource"></typeparam>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="source"></param>
  /// <param name="selector"></param>
  /// <returns></returns>
  public static ICreateStatement<TResult> PartitionBy<TSource, TResult>(this ICreateStatement<TSource> source, Expression<Func<TSource, TResult>> selector)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (selector == null)
      throw new ArgumentNullException(nameof(selector));

    return source.Provider.CreateStatement<TResult>(
      Expression.Call(
        null,
        PartitionByTSourceTResult(typeof(TSource), typeof(TResult)),
        source.Expression, Expression.Quote(selector)
      ));
  }

  #endregion

  #region ToStatementString

  public static string ToStatementString<TSource>(this ICreateStatement<TSource> source)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));

    var createStatement = source as CreateStatement<TSource>;

    var ksqlQuery = createStatement?.BuildKsql();

    return ksqlQuery ?? string.Empty;
  }

  #endregion
}
