using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;
using IQbservable = Kafka.DotNet.ksqlDB.KSql.Linq.IQbservable;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal abstract class KStreamSet : KSet, IQbservable
  {
    public IKSqlQbservableProvider Provider { get; internal set; }
    
    internal QueryContext QueryContext { get; set; }

    internal IScheduler ObserveOnScheduler { get; set; }
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

      var querySubscription = RunStreamAsObservable(cancellationTokenSource)
        .ObserveOn(ObserveOnScheduler ?? Scheduler.Default)
        .Subscribe(observer);

      var compositeDisposable = new CompositeDisposable
      {
        Disposable.Create(() => cancellationTokenSource.Cancel()), 
        querySubscription
      };

      return compositeDisposable;
    }

    internal IAsyncEnumerable<TEntity> RunStreamAsAsyncEnumerable(CancellationToken cancellationToken = default)
    {
      using var scope = serviceScopeFactory.CreateScope();

      var dependencies = scope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();

      var queryParameters = dependencies.QueryStreamParameters;
      queryParameters.Sql = dependencies.KSqlQueryGenerator.BuildKSql(Expression, QueryContext);


      queryParameters = TryOverrideAutoOffsetResetPolicy(queryParameters);

      return dependencies.KsqlDBProvider
        .Run<TEntity>(queryParameters, cancellationToken);
    }

    private IQueryParameters TryOverrideAutoOffsetResetPolicy(IQueryParameters queryParameters)
    {
      if (!QueryContext.AutoOffsetReset.HasValue) return queryParameters;
      
      if (queryParameters is QueryStreamParameters queryStreamParameters)
      {
        queryStreamParameters = queryStreamParameters.Clone();
        queryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName] = QueryContext.AutoOffsetReset.Value.ToString().ToLower();

        queryParameters = queryStreamParameters;
      }

      if (queryParameters is not QueryParameters qp) return queryParameters;
      
      queryParameters = qp.Clone();
      queryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName] = QueryContext.AutoOffsetReset.Value.ToString().ToLower();

      return queryParameters;
    }

    internal IObservable<TEntity> RunStreamAsObservable(CancellationTokenSource cancellationTokenSource)
    {
      var observableStream = RunStreamAsAsyncEnumerable(cancellationTokenSource.Token)
        .ToObservable();

      return observableStream;
    }

    internal string BuildKsql()
    {
      serviceScope = serviceScopeFactory.CreateScope();
      
      var dependencies = serviceScope.ServiceProvider.GetService<IKStreamSetDependencies>();

      var ksqlQuery = dependencies.KSqlQueryGenerator?.BuildKSql(Expression, QueryContext);
      
      serviceScope.Dispose();

      return ksqlQuery;
    }
  }
}