using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.Query.PullQueries;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// KSqlDBContext enables the creation of push and pull queries.
/// </summary>
public class KSqlDBContext : KSqlDBContextDependenciesProvider, IKSqlDBContext
{
  private readonly ModelBuilder modelBuilder;

  /// <summary>
  /// Initializes a new instance of the KSqlDBContext class with the specified ksqlDB server URL.
  /// </summary>
  /// <param name="ksqlDbUrl">The URL of the ksqlDB server.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public KSqlDBContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
    : this(new KSqlDBContextOptions(ksqlDbUrl), loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the KSqlDBContext class with the specified ksqlDB server URL.
  /// </summary>
  /// <param name="ksqlDbUrl">The URL of the ksqlDB server.</param>
  /// <param name="modelBuilder">The model builder.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public KSqlDBContext(string ksqlDbUrl, ModelBuilder modelBuilder, ILoggerFactory? loggerFactory = null)
    : this(new KSqlDBContextOptions(ksqlDbUrl), modelBuilder, loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the KSqlDBContext class with the specified context options.
  /// </summary>
  /// <param name="contextOptions">The options for configuring the KSqlDBContext.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public KSqlDBContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
    : this(contextOptions, new ModelBuilder(), loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the KSqlDBContext class with the specified context options.
  /// </summary>
  /// <param name="contextOptions">The options for configuring the KSqlDBContext.</param>
  /// <param name="modelBuilder">The model builder.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public KSqlDBContext(KSqlDBContextOptions contextOptions, ModelBuilder modelBuilder, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
    this.modelBuilder = modelBuilder;
    ContextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));
  }
  
  internal KSqlDBContextOptions ContextOptions { get; }

  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    base.OnConfigureServices(serviceCollection, contextOptions);

    serviceCollection.TryAddSingleton(modelBuilder);
    serviceCollection.TryAddSingleton<IMetadataProvider>(modelBuilder);
    serviceCollection.RegisterEndpointDependencies(contextOptions);
  }

  /// <summary>
  /// Creates a push stream for retrieving entities asynchronously.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entities to retrieve.</typeparam>
  /// <param name="queryParameters">The parameters for the push query.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
  /// <returns>An asynchronous enumerable of entities representing the push query.</returns>
  public IAsyncEnumerable<TEntity> CreatePushQuery<TEntity>(IKSqlDbParameters queryParameters, CancellationToken cancellationToken = default)
  {
    if (ContextOptions.EndpointType == EndpointType.Query && queryParameters is not QueryParameters)
      throw new InvalidOperationException($"Invalid {nameof(IKSqlDbParameters)} for endpoint {EndpointType.Query}");
#if !NETSTANDARD
    if (ContextOptions.EndpointType == EndpointType.QueryStream && queryParameters is not QueryStreamParameters)
      throw new InvalidOperationException($"Invalid {nameof(IKSqlDbParameters)} for endpoint {EndpointType.QueryStream}");
#endif

    var serviceScopeFactory = ServiceScopeFactory();

    var ksqlDbProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IKSqlDbProvider>();

    return ksqlDbProvider.Run<TEntity>(queryParameters, cancellationToken);
  }

  /// <summary>
  /// Creates a push query.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="fromItemName">Overrides the name of the stream or table which by default is derived from TEntity</param>
  /// <returns>A Qbservable for query composition and execution.</returns>
  public IQbservable<TEntity> CreatePushQuery<TEntity>(string? fromItemName = null)
  {
    var serviceScopeFactory = ServiceScopeFactory();

    if (fromItemName == String.Empty)
      fromItemName = null;

    var queryStreamContext = new QueryContext
    {
      ModelBuilder = modelBuilder,
      FromItemName = fromItemName
    };

    return new KQueryStreamSet<TEntity>(serviceScopeFactory, queryStreamContext);
  }

  #region CreateStatements

  /// <summary>
  /// Create a new materialized stream view, along with the corresponding Kafka topic, and stream the result of the query into the topic.
  /// </summary>
  /// <param name="streamName">Name of the stream to create.</param>
  /// <returns>An instance of IWithOrAsClause to continue building the stream statement.</returns>
  public IWithOrAsClause CreateStreamStatement(string streamName)
  {
    return CreateStatement(streamName, CreationType.Create, KSqlEntityType.Stream);
  }

  /// <summary>
  /// Create or replace a materialized stream view, along with the corresponding Kafka topic, and stream the result of the query into the topic.
  /// </summary>
  /// <param name="streamName">Name of the stream to create or replace.</param>
  /// <returns>An instance of IWithOrAsClause to continue building the stream statement.</returns>
  public IWithOrAsClause CreateOrReplaceStreamStatement(string streamName)
  {
    return CreateStatement(streamName, CreationType.CreateOrReplace, KSqlEntityType.Stream);
  }

  /// <summary>
  /// Create a new ksqlDB materialized table view, along with the corresponding Kafka topic, and stream the result of the query as a changelog into the topic.
  /// </summary>
  /// <param name="tableName">Name of the table to create.</param>
  /// <returns>An instance of IWithOrAsClause to continue building the stream statement.</returns>
  public IWithOrAsClause CreateTableStatement(string tableName)
  {
    return CreateStatement(tableName, CreationType.Create, KSqlEntityType.Table);
  }

  /// <summary>
  /// Create or replace a ksqlDB materialized table view, along with the corresponding Kafka topic, and stream the result of the query as a changelog into the topic.
  /// </summary>
  /// <param name="tableName">Name of the table to create or replace.</param>
  /// <returns>An instance of IWithOrAsClause to continue building the stream statement.</returns>
  public IWithOrAsClause CreateOrReplaceTableStatement(string tableName)
  {
    return CreateStatement(tableName, CreationType.CreateOrReplace, KSqlEntityType.Table);
  }

  private WithOrAsClause CreateStatement(string fromItemName, CreationType creationType, KSqlEntityType entityType)
  {
    var serviceScopeFactory = ServiceScopeFactory();

    var statementContext = new StatementContext
    {
      ModelBuilder = modelBuilder,
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
  /// <returns>An <see cref="IPullable{TEntity}"/> for query composition and execution.</returns>
  public IPullable<TEntity> CreatePullQuery<TEntity>(string? tableName = null)
  {
    var serviceScopeFactory = ServiceScopeFactory();

    if (tableName == String.Empty)
      tableName = null;

    var queryContext = new QueryContext
    {
      ModelBuilder = modelBuilder,
      FromItemName = tableName
    };

    return new KPullSet<TEntity>(serviceScopeFactory, queryContext);
  }

  /// <summary>
  /// Executes a pull query with the specified KSQL statement and retrieves the result as a single entity.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity to retrieve.</typeparam>
  /// <param name="ksql">The KSQL statement representing the pull query.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
  /// <returns>A ValueTask representing the asynchronous operation. The result is the retrieved entity.</returns>
  public ValueTask<TEntity?> ExecutePullQuery<TEntity>(string ksql, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrEmpty(ksql))
      throw new ArgumentException($"The {nameof(ksql)} statement cannot be null or empty.", nameof(ksql));

    var serviceScopeFactory = ServiceScopeFactory();

    using var scope = serviceScopeFactory.CreateScope();

    var dependencies = scope.ServiceProvider.GetRequiredService<IKPullSetDependencies>();

    var queryParameters = dependencies.QueryStreamParameters;
    queryParameters.Sql = ksql;

    return dependencies.KSqlDbProvider
      .Run<TEntity>(queryParameters, cancellationToken)
      .FirstOrDefaultAsync(cancellationToken: cancellationToken);
  }

  #endregion

  #region SaveChanges

  internal readonly ChangesCache ChangesCache = new();

  /// <summary>
  /// Add entity for insertion. In order to save them call SaveChangesAsync.
  /// </summary>
  /// <typeparam name="T">Type of entity to add.</typeparam>
  /// <param name="entity">Entity to add.</param>
  /// <param name="insertProperties">Optional insert properties.</param>
  public void Add<T>(T entity, InsertProperties? insertProperties = null)
  {
    Add(new InsertValues<T>(entity), insertProperties);
  }

  /// <summary>
  /// Add entity for insertion. In order to save them call SaveChangesAsync.
  /// </summary>
  /// <typeparam name="T">Type of entity to add.</typeparam>
  /// <param name="insertValues">Configurable insert values.</param>
  /// <param name="insertProperties">Optional insert properties.</param>
  public void Add<T>(InsertValues<T> insertValues, InsertProperties? insertProperties = null)
  {
    var serviceScopeFactory = ServiceScopeFactory();

    using var scope = serviceScopeFactory.CreateScope();

    var restApiClient = scope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    var statement = restApiClient.ToInsertStatement(insertValues, insertProperties);

    ChangesCache.Enqueue(statement);
  }

  private readonly CancellationTokenSource cts = new();

  /// <summary>
  /// Asynchronously saves the changes made in the context to the underlying data store.
  /// </summary>
  /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
  /// <returns>A task representing the asynchronous save operation. The task result is an HttpResponseMessage.</returns>
  public async Task<HttpResponseMessage?> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    if (ChangesCache.IsEmpty)
      return null;

    var serviceScopeFactory = ServiceScopeFactory();

    using var scope = serviceScopeFactory.CreateScope();

    var restApiClient = scope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    return await ChangesCache.SaveChangesAsync(restApiClient, cancellationToken).ConfigureAwait(false);
  }

  #endregion

  protected override async ValueTask OnDisposeAsync()
  {
    cts.Dispose();

    await base.OnDisposeAsync();

    Dispose(false);
  }

  protected override void Dispose(bool disposing)
  {
    if (!disposing) return;
    base.Dispose(true);
  }
}
