using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.Linq;

public static class QbservableExtensions
{
  #region Select

  private static MethodInfo selectTSourceTResult;

  private static MethodInfo SelectTSourceTResult(Type TSource, Type TResult) =>
    (selectTSourceTResult ??= new Func<IQbservable<object>, Expression<Func<object, object>>, IQbservable<object>>(Select).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource, TResult);

  /// <summary>
  /// Projects each element of a stream into a new form.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source.</typeparam>
  /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="selector">A transform function to apply to each source element.</param>
  /// <returns>An continuous sequence whose elements are the result of invoking the transform function on each element of source.</returns>
  public static IQbservable<TResult> Select<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector)
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

  #region WithOffsetResetPolicy

  private static MethodInfo withOffsetResetPolicyTResult;

  private static MethodInfo WithOffsetResetPolicyTResult(Type TSource) =>
    (withOffsetResetPolicyTResult ??= new Func<IQbservable<object>, AutoOffsetReset, IQbservable<object>>(WithOffsetResetPolicy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource);

  /// <summary>
  /// Determines what to do when there is no initial offset in Apache Kafka® or if the current offset doesn't exist on the server. The default value in ksqlDB is latest, which means all Kafka topics are read from the latest available offset.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="autoOffsetReset">Earliest or the latest offset.</param>
  /// <returns>The original sequence.</returns>
  public static IQbservable<TSource> WithOffsetResetPolicy<TSource>(this IQbservable<TSource> source, AutoOffsetReset autoOffsetReset)
  {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        WithOffsetResetPolicyTResult(typeof(TSource)),
        source.Expression, Expression.Constant(autoOffsetReset)
      ));
  }

  #endregion

  #region Where

  private static MethodInfo whereTSource;

  private static MethodInfo WhereTSource(Type TSource) =>
    (whereTSource ??= new Func<IQbservable<object>, Expression<Func<object, bool>>, IQbservable<object>>(Where).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource);

  /// <summary>
  /// Filters records that fulfill a specified condition.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">An observable sequence whose elements to filter.</param>
  /// <param name="predicate">A function to test each source element for a condition.</param>
  /// <returns>An observable sequence that contains elements from the input sequence that satisfy the condition.</returns>
  public static IQbservable<TSource> Where<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
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

  private static MethodInfo takeTSource;

  private static MethodInfo TakeTSource(Type TSource) =>
    (takeTSource ??= new Func<IQbservable<object>, int, IQbservable<object>>(Take).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource);

  /// <summary>
  /// Returns a specified number of contiguous elements from the start of an observable sequence.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="count">The number of elements to return.</param>
  /// <returns>An observable sequence that contains the specified number of elements from the start of the input sequence.</returns>
  public static IQbservable<TSource> Take<TSource>(this IQbservable<TSource> source, int count)
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
  /// Generates a string representation of the query used.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <returns>String representation of the push query.</returns>
  public static string ToQueryString<TSource>(this IQbservable<TSource> source)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));

    var kStreamSet = source as KStreamSet<TSource>;

    var ksqlQuery = kStreamSet?.BuildKsql();

    return ksqlQuery;
  }

  #endregion

  #region ExplainAsStringAsync

  internal static async Task<HttpResponseMessage> ExplainInternalAsync<TSource>(this IQbservable<TSource> source, CancellationToken cancellationToken = default)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));

    var kStreamSet = source as KStreamSet<TSource>;

    var explainStatement = CreateExplainStatement(kStreamSet);

    var httpClientFactory = kStreamSet.GetHttpClientFactory();

    var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(explainStatement), cancellationToken);

    return httpResponseMessage;
  }

  internal static string CreateExplainStatement<TSource>(KStreamSet<TSource> kStreamSet)
  {
    var ksqlQuery = kStreamSet?.BuildKsql();

    string explainStatement = StatementTemplates.Explain($"{ksqlQuery}");

    return explainStatement;
  }

  /// <summary>
  /// Show the execution plan for a SQL expression, show the execution plan plus additional runtime information and metrics.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
  /// <returns>Json with execution plan plus additional runtime information and metrics.</returns>
  public static async Task<string> ExplainAsStringAsync<TSource>(this IQbservable<TSource> source, CancellationToken cancellationToken = default)
  {
    var httpResponseMessage = await source.ExplainInternalAsync(cancellationToken);

    return await httpResponseMessage.Content.ReadAsStringAsync();
  }

  #endregion

  #region ExplainAsync
    
  /// <summary>
  /// Show the execution plan for a SQL expression, show the execution plan plus additional runtime information and metrics.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">The sequence to take elements from.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>ExplainResponse with execution plan plus additional runtime information and metrics.</returns>
  public static async Task<ExplainResponse[]> ExplainAsync<TSource>(this IQbservable<TSource> source, CancellationToken cancellationToken = default)
  {
    var httpResponseMessage = await source.ExplainInternalAsync(cancellationToken);

    return await httpResponseMessage.ToStatementResponsesAsync<ExplainResponse>();
  }

  #endregion

  #region ToObservable

  /// <summary>
  /// Runs the ksqlDb query as an observable sequence.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source stream.</typeparam>
  /// <param name="source">ksqlDb query to convert to an observable sequence.</param>
  /// <returns>The observable sequence whose elements are pushed from the given query.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static IObservable<TSource> ToObservable<TSource>(this IQbservable<TSource> source)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));

    return Observable.Defer(() =>
    {
      var streamSet = source as KStreamSet<TSource>;

      var cancellationTokenSource = new CancellationTokenSource();

      var observable = streamSet?.RunStreamAsObservable(cancellationTokenSource);

      return observable?.Finally(() => cancellationTokenSource.Cancel());
    });
  }

  #endregion

  #region ToAsyncEnumerable

  /// <summary>
  /// Runs the query as an async-enumerable sequence.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source stream.</typeparam>
  /// <param name="source">An ksqlDb query to subscribe to.</param>
  /// <returns>An async-enumerable sequence for the query.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IQbservable<TSource> source)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));

    var streamSet = source as KStreamSet<TSource>;

    return streamSet?.RunStreamAsAsyncEnumerable();
  }

  #endregion

  #region Subscribe delegate-based overloads

  /// <summary>
  /// Subscribes an element handler and an exception handler to an qbservable stream.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the source stream.</typeparam>
  /// <param name="source">Observable stream to subscribe to.</param>
  /// <param name="onNext">Action to invoke for each element in the qbservable stream.</param>
  /// <param name="onError">Action to invoke upon exceptional termination of the qbservable stream.</param>
  /// <returns><see cref="IDisposable"/> object used to unsubscribe from the qbservable stream.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> or <paramref name="onError"/> is <c>null</c>.</exception>
  public static IDisposable Subscribe<T>(this IQbservable<T> source, Action<T> onNext, Action<Exception> onError)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (onNext == null)
      throw new ArgumentNullException(nameof(onNext));

    if (onError == null)
      throw new ArgumentNullException(nameof(onError));

    return source.Subscribe(new AnonymousObserver<T>(onNext, onError, () => { }));
  }

  /// <summary>
  /// Subscribes an element handler, an exception handler, and a completion handler to an qbservable stream.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the source stream.</typeparam>
  /// <param name="source">Observable sequence to subscribe to.</param>
  /// <param name="onNext">Action to invoke for each element in the qbservable stream.</param>
  /// <param name="onError">Action to invoke upon exceptional termination of the qbservable stream.</param>
  /// <param name="onCompleted">Action to invoke upon graceful termination of the qbservable stream.</param>
  /// <returns><see cref="IDisposable"/> object used to unsubscribe from the qbservable stream.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> or <paramref name="onError"/> or <paramref name="onCompleted"/> is <c>null</c>.</exception>
  public static IDisposable Subscribe<T>(this IQbservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (onNext == null)
      throw new ArgumentNullException(nameof(onNext));

    if (onError == null)
      throw new ArgumentNullException(nameof(onError));

    if (onCompleted == null)
      throw new ArgumentNullException(nameof(onCompleted));

    return source.Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));
  }

  internal static IDisposable Subscribe<T>(this IQbservable<T> source, Action<T> onNext)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (onNext == null)
      throw new ArgumentNullException(nameof(onNext));

    return source.Subscribe(new AnonymousObserver<T>(onNext, e => throw e, () => { }));
  }

  #endregion

  #region ObserveOn

  /// <summary>
  /// Wraps the source sequence in order to run its observer callbacks on the specified scheduler.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">Source sequence.</param>
  /// <param name="scheduler">Scheduler to notify observers on.</param>
  /// <returns>The source sequence whose observations happen on the specified scheduler.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="scheduler"/> is null.</exception>
  public static IQbservable<TSource> ObserveOn<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (scheduler == null)
      throw new ArgumentNullException(nameof(scheduler));

    if (source is KStreamSet streamSet)
      streamSet.ObserveOnScheduler = scheduler;

    return source;
  }

  #endregion

  #region SubscribeOn

  /// <summary>
  /// Wraps the source sequence in order to run its subscription on the specified scheduler.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">Source sequence.</param>
  /// <param name="scheduler">Scheduler to perform subscription actions on.</param>
  /// <returns>The original source.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="scheduler"/> is null.</exception>
  public static IQbservable<TSource> SubscribeOn<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (scheduler == null)
      throw new ArgumentNullException(nameof(scheduler));

    if (source is KStreamSet streamSet)
      streamSet.SubscribeOnScheduler = scheduler;

    return source;
  }

  #endregion

  #region SubscribeAsync

  /// <summary>
  /// Subscribes an element handler, an exception handler, and a completion handler to an qbservable stream.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the source stream.</typeparam>
  /// <param name="source">Observable sequence to subscribe to.</param>
  /// <param name="onNext">Action to invoke for each element in the qbservable stream.</param>
  /// <param name="onError">Action to invoke upon exceptional termination of the qbservable stream.</param>
  /// <param name="onCompleted">Action to invoke upon graceful termination of the qbservable stream.</param>
  /// <returns>Subscription with query id.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> or <paramref name="onError"/> or <paramref name="onCompleted"/> is <c>null</c>.</exception>
  public static Task<Subscription> SubscribeAsync<T>(this IQbservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken = default)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (onNext == null)
      throw new ArgumentNullException(nameof(onNext));

    if (onError == null)
      throw new ArgumentNullException(nameof(onError));

    if (onCompleted == null)
      throw new ArgumentNullException(nameof(onCompleted));

    return source.SubscribeAsync(new AnonymousObserver<T>(onNext, onError, onCompleted), cancellationToken);
  }

  #endregion

  #region GroupBy

  private static MethodInfo groupByTSourceTKey;

  private static MethodInfo GroupByTSourceTKey(Type TSource, Type TKey) =>
    (groupByTSourceTKey ??= new Func<IQbservable<object>, Expression<Func<object, object>>, IQbservable<IKSqlGrouping<object, object>>>(GroupBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource, TKey);

  /// <summary>
  /// Group records in a window. Required by the WINDOW clause. Windowing queries must group by the keys that are selected in the query.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <typeparam name="TKey">The type of the grouping key computed for each element in the source sequence.</typeparam>
  /// <param name="source">An observable sequence whose elements to group.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <returns>Grouped elements of an observable sequence according to a specified key selector function.</returns>
  public static IQbservable<IKSqlGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

    return source.Provider.CreateQuery<IKSqlGrouping<TKey, TSource>>(
      Expression.Call(
        null,
        GroupByTSourceTKey(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Quote(keySelector))
    );
  }
    
  private static MethodInfo groupByTSourceTKeyTElement3;

  private static MethodInfo GroupBy_TSource_TKey_TElement_3(Type TSource, Type TKey, Type TElement) =>
    (groupByTSourceTKeyTElement3 ??= new Func<IQbservable<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, IQbservable<IKSqlGrouping<object, object>>>(GroupBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource, TKey, TElement);

  internal static IQbservable<IKSqlGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (keySelector == null)throw new ArgumentNullException(nameof(keySelector));
    if (elementSelector == null)throw new ArgumentNullException(nameof(elementSelector));

    return source.Provider.CreateQuery<IKSqlGrouping<TKey, TElement>>(
      Expression.Call(
        null,
        GroupBy_TSource_TKey_TElement_3(typeof(TSource), typeof(TKey), typeof(TElement)),
        source.Expression, Expression.Quote(keySelector), Expression.Quote(elementSelector)
      ));
  }

  #endregion

  #region GroupJoin

  private static MethodInfo GetGroupJoinMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
  {
    return f.Method;
  }

  /// <summary>
  /// Correlates the elements of two sequences based on equality of keys and groups the results.
  /// </summary>
  /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
  /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
  /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
  /// <typeparam name="TResult">The type of the result elements.</typeparam>
  /// <param name="outer">The first sequence to join.</param>
  /// <param name="inner">The second sequence to join.</param>
  /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
  /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
  /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
  /// <returns>An IQbservable that contains elements of type TResult that are obtained by performing a grouped join on two sequences.</returns>
  public static IQbservable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQbservable<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IQbservable<TInner>, TResult>> resultSelector)
  {
    if (outer == null)
      throw new ArgumentNullException(nameof(outer));
      
    if (inner == null)
      throw new ArgumentNullException(nameof(inner));
      
    if (outerKeySelector == null)
      throw new ArgumentNullException(nameof(outerKeySelector));
      
    if (innerKeySelector == null)
      throw new ArgumentNullException(nameof(innerKeySelector));
      
    if (resultSelector == null)
      throw new ArgumentNullException(nameof(resultSelector));
      
    return outer.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        GetGroupJoinMethodInfo(GroupJoin, outer, inner, outerKeySelector, innerKeySelector, resultSelector),
        outer.Expression,
        inner.Expression,
        Expression.Quote(outerKeySelector), Expression.Quote(innerKeySelector), Expression.Quote(resultSelector)));
  }

  #endregion

  #region DefaultIfEmpty

  private static MethodInfo defaultIfEmptyTSource1;

  private static MethodInfo DefaultIfEmptyTSource1(Type source) =>
    (defaultIfEmptyTSource1 ??= new Func<IQbservable<object>, IQbservable<object>>(DefaultIfEmpty).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source);

  /// <summary>
  /// An observable sequence that contains the default value for the TSource type if the source is empty; otherwise, the elements of the source itself.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source</typeparam>
  /// <param name="source">The sequence to return the specified value for if it is empty.</param>
  public static IQbservable<TSource> DefaultIfEmpty<TSource>(this IQbservable<TSource> source)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        DefaultIfEmptyTSource1(typeof(TSource)), source.Expression));
  }

  #endregion

  #region SelectMany

  private static MethodInfo selectManyTSourceTCollectionTResult3;

  private static MethodInfo SelectManyTSourceTCollectionTResult3(Type source, Type collection, Type result) =>
    (selectManyTSourceTCollectionTResult3 ??= new Func<IQbservable<object>, Expression<Func<object, IQbservable<object>>>, Expression<Func<object, object, object>>, IQbservable<object>>(SelectMany).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(source, collection, result);


  /// <summary>
  /// Projects each element of an qbservable sequence to a sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <typeparam name="TSequence">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
  /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
  /// <param name="source">An observable sequence of elements to project.</param>
  /// <param name="collectionSelector">A transform function to apply to each element.</param>
  /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
  /// <returns>An observable sequence.</returns>
  public static IQbservable<TResult> SelectMany<TSource, TSequence, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IQbservable<TSequence>>> collectionSelector, Expression<Func<TSource, TSequence, TResult>> resultSelector)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));
    if (collectionSelector == null)
      throw new ArgumentNullException(nameof(collectionSelector));
    if (resultSelector == null)
      throw new ArgumentNullException(nameof(resultSelector));
      
    return source.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        SelectManyTSourceTCollectionTResult3(typeof(TSource), typeof(TSequence), typeof(TResult)),
        source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector)
      ));
  }

  #endregion

  #region Having

  private static MethodInfo havingTSource;

  private static MethodInfo HavingTSource(Type TSource, Type TKey) =>
    (havingTSource ??= new Func<IQbservable<IKSqlGrouping<object, object>>, Expression<Func<IKSqlGrouping<object, object>, bool>>, IQbservable<IKSqlGrouping<object, object>>>(Having).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource, TKey);

  /// <summary>
  /// Extract records from an aggregation that fulfill a specified condition.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <typeparam name="TKey">The type of the grouping key computed for each element in the source sequence.</typeparam>
  /// <param name="source">An observable sequence whose elements to group.</param>
  /// <param name="predicate">A function to extract the key for each element.</param>
  /// <returns>Extracted elements of an aggregation that satisfy the condition.</returns>
  public static IQbservable<IKSqlGrouping<TKey, TSource>> Having<TSource, TKey>(this IQbservable<IKSqlGrouping<TKey, TSource>> source, Expression<Func<IKSqlGrouping<TKey, TSource>, bool>> predicate)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return source.Provider.CreateQuery<IKSqlGrouping<TKey, TSource>>(
      Expression.Call(
        null,
        HavingTSource(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Quote(predicate)
      ));
  }

  #endregion

  #region WindowedBy

  private static MethodInfo windowedByTSourceTKey;

  private static MethodInfo WindowedByTSourceTKey(Type TSource, Type TKey) =>
    (windowedByTSourceTKey ??= new Func<IQbservable<IKSqlGrouping<object, object>>, TimeWindows, IQbservable<IWindowedKSql<object, object>>>(WindowedBy).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TSource, TKey);

  /// <summary>
  /// Group input records that have the same key into a window, for operations like aggregations and joins.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <typeparam name="TKey">The type of the grouping key computed for each element in the source sequence.</typeparam>
  /// <param name="source">An observable sequence whose elements to group.</param>
  /// <param name="timeWindows">Type of window TUMBLING, HOPPING, etc and its durations.</param>
  /// <returns>Grouped elements of an aggregation or join that satisfy the condition.</returns>
  public static IQbservable<IWindowedKSql<TKey, TSource>> WindowedBy<TSource, TKey>(this IQbservable<IKSqlGrouping<TKey, TSource>> source, TimeWindows timeWindows)
  {
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (timeWindows == null) throw new ArgumentNullException(nameof(timeWindows));

    return source.Provider.CreateQuery<IWindowedKSql<TKey, TSource>>(
      Expression.Call(null,
        WindowedByTSourceTKey(typeof(TSource), typeof(TKey)),
        source.Expression, Expression.Constant(timeWindows)));
  }

  #endregion

  #region Join

  private static MethodInfo joinTOuterTInnerTKeyTResult;

  private static MethodInfo JoinTOuterTInnerTKeyTResult(Type TOuter, Type TInner, Type TKey, Type TResult) =>
    (joinTOuterTInnerTKeyTResult ??= new Func<IQbservable<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, IQbservable<object>>(Join).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TOuter, TInner, TKey, TResult);

  /// <summary>
  /// Select records in a stream or table that have matching values in another stream or table. (INNER JOIN)
  /// </summary>
  /// <typeparam name="TOuter"></typeparam>
  /// <typeparam name="TInner"></typeparam>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="outer"></param>
  /// <param name="inner"></param>
  /// <param name="outerKeySelector"></param>
  /// <param name="innerKeySelector"></param>
  /// <param name="resultSelector"></param>
  /// <returns></returns>
  public static IQbservable<TResult> Join<TOuter, TInner, TKey, TResult>(this IQbservable<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        JoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region LeftJoin

  private static MethodInfo leftJoinTOuterTInnerTKeyTResult;

  private static MethodInfo LeftJoinTOuterTInnerTKeyTResult(Type TOuter, Type TInner, Type TKey, Type TResult) =>
    (leftJoinTOuterTInnerTKeyTResult ??= new Func<IQbservable<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, IQbservable<object>>(LeftJoin).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TOuter, TInner, TKey, TResult);

  /// <summary>
  /// Select all records from the left stream/table and the matched records from the right stream/table.
  /// </summary>
  /// <typeparam name="TOuter"></typeparam>
  /// <typeparam name="TInner"></typeparam>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="outer"></param>
  /// <param name="inner"></param>
  /// <param name="outerKeySelector"></param>
  /// <param name="innerKeySelector"></param>
  /// <param name="resultSelector"></param>
  /// <returns></returns>
  public static IQbservable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IQbservable<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        LeftJoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region RightJoin

  private static MethodInfo rightJoinTOuterTInnerTKeyTResult;

  private static MethodInfo RightJoinTOuterTInnerTKeyTResult(Type TOuter, Type TInner, Type TKey, Type TResult) =>
    (rightJoinTOuterTInnerTKeyTResult ??= new Func<IQbservable<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, IQbservable<object>>(RightJoin).GetMethodInfo().GetGenericMethodDefinition())
    .MakeGenericMethod(TOuter, TInner, TKey, TResult);

  /// <summary>
  /// Select all records for the right side of the join and the matching records from the left side. If the matching records on the left side are missing, the corresponding columns will contain null values. ksqldb v0.26.0
  /// </summary>
  /// <typeparam name="TOuter"></typeparam>
  /// <typeparam name="TInner"></typeparam>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="outer"></param>
  /// <param name="inner"></param>
  /// <param name="outerKeySelector"></param>
  /// <param name="innerKeySelector"></param>
  /// <param name="resultSelector"></param>
  /// <returns></returns>
  public static IQbservable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IQbservable<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        RightJoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion

  #region FullOuterJoin

  private static MethodInfo fullOuterJoinTOuterTInnerTKeyTResult;

  private static MethodInfo FullOuterJoinTOuterTInnerTKeyTResult(Type TOuter, Type TInner, Type TKey, Type TResult) =>
    (fullOuterJoinTOuterTInnerTKeyTResult ??= new Func<IQbservable<object>, ISource<object>, Expression<Func<object, object>>, Expression<Func<object, object>>, Expression<Func<object, object, object>>, IQbservable<object>>(FullOuterJoin).GetMethodInfo()?.GetGenericMethodDefinition())
    .MakeGenericMethod(TOuter, TInner, TKey, TResult);

  /// <summary>
  /// Select all records when there is a match in the left stream/table or the right stream/table records.
  /// </summary>
  /// <typeparam name="TOuter"></typeparam>
  /// <typeparam name="TInner"></typeparam>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="outer"></param>
  /// <param name="inner"></param>
  /// <param name="outerKeySelector"></param>
  /// <param name="innerKeySelector"></param>
  /// <param name="resultSelector"></param>
  /// <returns></returns>
  public static IQbservable<TResult> FullOuterJoin<TOuter, TInner, TKey, TResult>(this IQbservable<TOuter> outer, ISource<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
  {
    if (outer == null) throw new ArgumentNullException(nameof(outer));
    if (inner == null) throw new ArgumentNullException(nameof(inner));
    if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
    if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
    if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

    return outer.Provider.CreateQuery<TResult>(
      Expression.Call(
        null,
        FullOuterJoinTOuterTInnerTKeyTResult(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, resultSelector));
  }

  #endregion
}