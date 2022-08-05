using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.PullQueries;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if !NETSTANDARD
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// KSqlDBContext enables the creation of push and pull queries.
/// </summary>
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

  internal KSqlDBContextOptions ContextOptions => contextOptions;

  internal KSqlDBContextQueryDependenciesProvider KSqlDBQueryContext { get; set; }

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

  /// <summary>
  /// Creates a push query for the query-stream endpoint.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="fromItemName">Overrides the name of the stream or table which by default is derived from TEntity</param>
  /// <returns>A Qbservable for query composition and execution.</returns>
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

  /// <summary>
  /// Creates a push query for the query endpoint.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="fromItemName">Overrides the name of the stream or table which by default is derived from TEntity</param>
  /// <returns>A Qbservable for query composition and execution.</returns>
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

  /// <summary>
  /// Creates a pull query.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="tableName">Overrides the name of the table which by default is derived from TEntity</param>
  /// <returns>An IPullable for query composition and execution.</returns>
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

  /// <summary>
  /// Executes the provided ksql query.
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  /// <param name="ksql">The KSQL query to execute.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>The first item.</returns>
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

  #region SaveChanges

  private readonly ChangesCache changesCache = new();

  /// <summary>
  /// Add entity for insertion. In order to save them call SaveChangesAsync.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity">Entity to add</param>
  /// <param name="insertProperties">Optional insert properties.</param>
  public void Add<T>(T entity, InsertProperties insertProperties = null)
  {
    var serviceScopeFactory = Initialize(contextOptions);

    using var scope = serviceScopeFactory.CreateScope();

    var restApiClient = scope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    var statement = restApiClient.ToInsertStatement(entity, insertProperties);

    changesCache.Enqueue(statement);
  }

  private readonly CancellationTokenSource cts = new();

  /// <summary>
  /// Save the entities added to context.
  /// </summary>
  /// <returns>Save response.</returns>
  public async Task<HttpResponseMessage> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    if (changesCache.IsEmpty)
      return null;

    var serviceScopeFactory = Initialize(contextOptions);

    using var scope = serviceScopeFactory.CreateScope();

    var restApiClient = scope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    return await changesCache.SaveChangesAsync(restApiClient, cancellationToken).ConfigureAwait(false);
  }

  #endregion

  protected override async ValueTask OnDisposeAsync()
  {
    cts.Dispose();

#if !NETSTANDARD
    await base.OnDisposeAsync();
#endif
    if (KSqlDBQueryContext != null)
      await KSqlDBQueryContext.DisposeAsync().ConfigureAwait(false);
    Dispose(false);
  }

  protected override void Dispose(bool disposing)
  {
    if (!disposing) return;

    (KSqlDBQueryContext as IDisposable)?.Dispose();
    base.Dispose(true);
  }
}
