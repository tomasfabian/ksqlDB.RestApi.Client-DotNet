using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal abstract class KStreamSet : KSet, Linq.IQbservable
{
  public IKSqlQbservableProvider Provider { get; internal set; } = null!;

  internal QueryContext QueryContext { get; set; } = null!;
    
  internal IScheduler? ObserveOnScheduler { get; set; }
    
  internal IScheduler? SubscribeOnScheduler { get; set; }
}

internal abstract class KStreamSet<TEntity> : KStreamSet, Linq.IQbservable<TEntity>
{
  private readonly IServiceScopeFactory serviceScopeFactory;
  protected KStreamSet(IServiceScopeFactory serviceScopeFactory, QueryContext? queryContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
    QueryContext = queryContext ?? new QueryContext();

    Provider = new QbservableProvider(serviceScopeFactory, queryContext);
      
    Expression = Expression.Constant(this);
  }

  protected KStreamSet(IServiceScopeFactory serviceScopeFactory, Expression expression, QueryContext? queryContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
    QueryContext = queryContext ?? new QueryContext();

    Provider = new QbservableProvider(serviceScopeFactory, queryContext);

    Expression = expression ?? throw new ArgumentNullException(nameof(expression));
  }

  public override Type ElementType => typeof(TEntity);

  public IDisposable Subscribe(IObserver<TEntity> observer)
  {
    var cancellationTokenSource = new CancellationTokenSource();

    var observable = RunStreamAsObservable(cancellationTokenSource);
      
    observable = TryApplySchedulers(observable);
      
    var querySubscription = observable.Subscribe(observer);

    var compositeDisposable = new CompositeDisposable
    {
      Disposable.Create(() => cancellationTokenSource.Cancel()), 
      querySubscription
    };

    return compositeDisposable;
  }

  public async Task<Subscription> SubscribeAsync(IObserver<TEntity> observer, CancellationToken cancellationToken = default)
  {
    var query = await RunStreamAsObservableAsync(cancellationToken).ConfigureAwait(false);

    var observable = query.Source;

    observable = TryApplySchedulers(observable);

    observable.Subscribe(observer, cancellationToken);
      
    return new Subscription { QueryId = query.QueryId };
  }

  private IObservable<TEntity> TryApplySchedulers(IObservable<TEntity> observable)
  {
    if (SubscribeOnScheduler != null)
      observable = observable.SubscribeOn(SubscribeOnScheduler);

    if (ObserveOnScheduler != null)
      observable = observable.ObserveOn(ObserveOnScheduler);

    return observable;
  }

  internal IAsyncEnumerable<TEntity> RunStreamAsAsyncEnumerable(CancellationToken cancellationToken = default)
  {
    using var scope = serviceScopeFactory.CreateScope();

    var dependencies = scope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();
      
    var queryParameters = GetQueryParameters(dependencies);

    return dependencies.KSqlDbProvider
      .Run<TEntity>(queryParameters, cancellationToken);
  }

  internal Task<QueryStream<TEntity>> RunStreamAsAsyncEnumerableAsync(CancellationToken cancellationToken = default)
  {
    using var scope = serviceScopeFactory.CreateScope();

    var dependencies = scope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();

    var queryParameters = GetQueryParameters(dependencies);

    return dependencies.KSqlDbProvider
      .RunAsync<TEntity>(queryParameters, cancellationToken);
  }

  private IQueryParameters GetQueryParameters(IKStreamSetDependencies dependencies)
  {
    var queryParameters = dependencies.QueryStreamParameters;
      
    queryParameters.Sql = dependencies.KSqlQueryGenerator.BuildKSql(Expression, QueryContext);

    queryParameters = TryOverrideAutoOffsetResetPolicy(queryParameters);

    return queryParameters;
  }

  private IKSqlDbParameters TryOverrideAutoOffsetResetPolicy(IKSqlDbParameters queryParameters)
  {
    if (!QueryContext.AutoOffsetReset.HasValue) return queryParameters;
      
    var overridenParameters = queryParameters.Clone();
    ((IPushQueryParameters)overridenParameters).AutoOffsetReset = QueryContext.AutoOffsetReset.Value;

    return overridenParameters;
  }

  internal IObservable<TEntity> RunStreamAsObservable(CancellationTokenSource cancellationTokenSource)
  {
    var query = RunStreamAsAsyncEnumerable(cancellationTokenSource.Token);

    var observableStream = ksqlDb.RestApi.Client.KSql.Linq.AsyncEnumerable.ToObservable(query);

    return observableStream;
  }
    
  internal async Task<(string? QueryId, IObservable<TEntity> Source)> RunStreamAsObservableAsync(CancellationToken cancellationTokenSource = default)
  {
    var query = await RunStreamAsAsyncEnumerableAsync(cancellationTokenSource).ConfigureAwait(false);

    var observableStream = ksqlDb.RestApi.Client.KSql.Linq.AsyncEnumerable.ToObservable(query.EnumerableQuery);

    return (query.QueryId, observableStream);
  }

  internal string BuildKsql()
  {
    var serviceScope = serviceScopeFactory.CreateScope();
      
    var dependencies = serviceScope.ServiceProvider.GetService<IKStreamSetDependencies>();
      
    var ksqlQuery = dependencies?.KSqlQueryGenerator.BuildKSql(Expression, QueryContext);
      
    serviceScope.Dispose();

    return ksqlQuery ?? string.Empty;
  }

  internal IHttpClientFactory GetHttpClientFactory()
  {
    using var scope = serviceScopeFactory.CreateScope();
    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

    return httpClientFactory;
  }

  internal Task<HttpResponseMessage> ExecuteAsync(string ksql, CancellationToken cancellationToken = default)
  {
    using var scope = serviceScopeFactory.CreateScope();
    var restApiClient = scope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    return restApiClient.ExecuteStatementAsync(new(ksql), cancellationToken);
  }
}
