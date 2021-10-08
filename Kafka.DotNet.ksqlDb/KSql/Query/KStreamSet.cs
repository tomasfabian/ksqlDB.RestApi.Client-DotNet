using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using IQbservable = Kafka.DotNet.ksqlDB.KSql.Linq.IQbservable;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal abstract class KStreamSet : KSet, IQbservable
  {
    public IKSqlQbservableProvider Provider { get; internal set; }
    
    internal QueryContext QueryContext { get; set; }
    
    internal IScheduler ObserveOnScheduler { get; set; }
    
    internal IScheduler SubscribeOnScheduler { get; set; }
  }

  internal abstract class KStreamSet<TEntity> : KStreamSet, Linq.IQbservable<TEntity>
  {
    private readonly IServiceScopeFactory serviceScopeFactory;
    private IServiceScope serviceScope;

    protected KStreamSet(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
      QueryContext = queryContext;

      Provider = new QbservableProvider(serviceScopeFactory, queryContext);
      
      Expression = Expression.Constant(this);
    }

    protected KStreamSet(IServiceScopeFactory serviceScopeFactory, Expression expression, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
      QueryContext = queryContext;

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
      
      var credentials = QueryContext.Credentials;
      dependencies.KsqlDBProvider.SetCredentials(credentials);

      return dependencies.KsqlDBProvider
        .Run<TEntity>(queryParameters, cancellationToken);
    }

    internal Task<QueryStream<TEntity>> RunStreamAsAsyncEnumerableAsync(CancellationToken cancellationToken = default)
    {
      using var scope = serviceScopeFactory.CreateScope();

      var dependencies = scope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();

      var queryParameters = GetQueryParameters(dependencies);

      var credentials = QueryContext.Credentials;

      dependencies.KsqlDBProvider.SetCredentials(credentials);

      return dependencies.KsqlDBProvider
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
      overridenParameters.AutoOffsetReset = QueryContext.AutoOffsetReset.Value;

      return overridenParameters;
    }

    internal IObservable<TEntity> RunStreamAsObservable(CancellationTokenSource cancellationTokenSource)
    {
      var query = RunStreamAsAsyncEnumerable(cancellationTokenSource.Token);

      var observableStream = query.ToObservable();

      return observableStream;
    }
    
    internal async Task<(string QueryId, IObservable<TEntity> Source)> RunStreamAsObservableAsync(CancellationToken cancellationTokenSource = default)
    {
      var query = await RunStreamAsAsyncEnumerableAsync(cancellationTokenSource).ConfigureAwait(false);

      var observableStream = query.EnumerableQuery.ToObservable();

      return (query.QueryId, observableStream);
    }

    internal string BuildKsql()
    {
      serviceScope = serviceScopeFactory.CreateScope();
      
      var dependencies = serviceScope.ServiceProvider.GetService<IKStreamSetDependencies>();
      
      var ksqlQuery = dependencies.KSqlQueryGenerator?.BuildKSql(Expression, QueryContext);
      
      serviceScope.Dispose();

      return ksqlQuery;
    }

    internal IHttpClientFactory GetHttpClientFactory()
    {
      using var scope = serviceScopeFactory.CreateScope();
      var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

      return httpClientFactory;
    }
  }
}