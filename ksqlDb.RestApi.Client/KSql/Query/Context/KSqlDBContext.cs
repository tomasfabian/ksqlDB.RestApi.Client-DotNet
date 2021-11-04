using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.PullQueries;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if !NETSTANDARD
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public class KSqlDBContext : KSqlDBContextDependenciesProvider, IKSqlDBContext
  {
    private readonly KSqlDBContextOptions contextOptions;

    public KSqlDBContext(string ksqlDbUrl, ILoggerFactory loggerFactory = null)
      : this(new KSqlDBContextOptions(ksqlDbUrl), loggerFactory)
    {
    }

    public KSqlDBContext(KSqlDBContextOptions contextOptions, ILoggerFactory loggerFactory = null)
      : base(contextOptions, loggerFactory)
    {
      this.contextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));

      KSqlDBQueryContext = new KSqlDBContextQueryDependenciesProvider(contextOptions);
    }
    
    internal readonly KSqlDBContextQueryDependenciesProvider KSqlDBQueryContext;
    
#if !NETSTANDARD

    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
    {
      base.OnConfigureServices(serviceCollection, contextOptions);
          
      serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryStreamProvider>();
          
      serviceCollection.TryAddSingleton<IKSqlDbParameters>(contextOptions.QueryStreamParameters);
    }

    public IAsyncEnumerable<TEntity> CreateQueryStream<TEntity>(QueryStreamParameters queryStreamParameters, CancellationToken cancellationToken = default)
    {
      var serviceScopeFactory = Initialize(contextOptions);

      var ksqlDBProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbProvider>();

      return ksqlDBProvider.Run<TEntity>(queryStreamParameters, cancellationToken);
    }

    public IQbservable<TEntity> CreateQueryStream<TEntity>(string fromItemName = null)
    {
      var serviceScopeFactory = Initialize(contextOptions);

      if (fromItemName == String.Empty)
        fromItemName = null;

      var queryStreamContext = new QueryContext
      {
        FromItemName = fromItemName
      };
      
      return new KQueryStreamSet<TEntity>(serviceScopeFactory, queryStreamContext);
    }

#endif
    
    public IAsyncEnumerable<TEntity> CreateQuery<TEntity>(QueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
      var serviceScopeFactory = KSqlDBQueryContext.Initialize(contextOptions);

      var ksqlDBProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbProvider>();

      return ksqlDBProvider.Run<TEntity>(queryParameters, cancellationToken);
    }

    public IQbservable<TEntity> CreateQuery<TEntity>(string fromItemName = null)
    {
      var serviceScopeFactory = KSqlDBQueryContext.Initialize(contextOptions);

      if (fromItemName == String.Empty)
        fromItemName = null;

      var queryStreamContext = new QueryContext
      {
        FromItemName = fromItemName
      };

      return new KQueryStreamSet<TEntity>(serviceScopeFactory, queryStreamContext);
    }

    #region CreateStatements

    public IWithOrAsClause CreateStreamStatement(string streamName)
    {
      return CreateStatement(streamName, CreationType.Create, KSqlEntityType.Stream);
    }

    public IWithOrAsClause CreateOrReplaceStreamStatement(string streamName)
    {
      return CreateStatement(streamName, CreationType.CreateOrReplace, KSqlEntityType.Stream);
    }

    public IWithOrAsClause CreateTableStatement(string tableName)
    {
      return CreateStatement(tableName, CreationType.Create, KSqlEntityType.Table);
    }

    public IWithOrAsClause CreateOrReplaceTableStatement(string tableName)
    {
      return CreateStatement(tableName, CreationType.CreateOrReplace, KSqlEntityType.Table);
    }

    private IWithOrAsClause CreateStatement(string fromItemName, CreationType creationType, KSqlEntityType entityType)
    {
      var serviceScopeFactory = KSqlDBQueryContext.Initialize(contextOptions);

      if (fromItemName == String.Empty)
        fromItemName = null;

      var statementContext = new StatementContext
      {
        EntityName = fromItemName, 
        CreationType = creationType, 
        KSqlEntityType = entityType
      };

      return new WithOrAsClause(serviceScopeFactory, statementContext);
    }

    #endregion

    #region Pull queries

    public IPullable<TEntity> CreatePullQuery<TEntity>(string tableName = null)
    {      
      var serviceScopeFactory = KSqlDBQueryContext.Initialize(contextOptions);

      if (tableName == String.Empty)
        tableName = null;

      var queryContext = new QueryContext
      {
        FromItemName = tableName
      };

      return new KPullSet<TEntity>(serviceScopeFactory, queryContext);
    }

    public ValueTask<TEntity> ExecutePullQuery<TEntity>(string ksql, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrEmpty(ksql))
        throw new ArgumentException(nameof(ksql));

      var serviceScopeFactory = KSqlDBQueryContext.Initialize(contextOptions);

      using var scope = serviceScopeFactory.CreateScope();

      var dependencies = scope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();

      var queryParameters = dependencies.QueryStreamParameters;
      queryParameters.Sql = ksql;

      return dependencies.KsqlDBProvider
        .Run<TEntity>(queryParameters, cancellationToken)
        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    #endregion

    protected override async ValueTask OnDisposeAsync()
    {
#if !NETSTANDARD
      await base.OnDisposeAsync();
#endif
      if(KSqlDBQueryContext != null)
        await KSqlDBQueryContext.DisposeAsync();
    }
  }
}