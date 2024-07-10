using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

public interface IKSqlDBContext : IKSqlDBStatementsContext, IAsyncDisposable, IDisposable
{
  /// <summary>
  /// Creates a push query.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="fromItemName">Overrides the name of the stream or table which by default is derived from TEntity</param>
  /// <returns>A Qbservable for query composition and execution.</returns>
  IQbservable<TEntity> CreatePushQuery<TEntity>(string? fromItemName = null);

  /// <summary>
  /// Creates a push query for retrieving entities asynchronously.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entities to retrieve.</typeparam>
  /// <param name="queryParameters">The parameters for the push query.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
  /// <returns>An asynchronous enumerable of entities representing the push query.</returns>
  IAsyncEnumerable<TEntity> CreatePushQuery<TEntity>(IKSqlDbParameters queryParameters, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a pull query.
  /// </summary>
  /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
  /// <param name="tableName">Overrides the name of the table which by default is derived from TEntity</param>
  /// <returns>An <see cref="IPullable{TEntity}"/> for query composition and execution.</returns>
  IPullable<TEntity> CreatePullQuery<TEntity>(string? tableName = null);

  /// <summary>
  /// Executes a pull query with the specified KSQL statement and retrieves the result as a single entity.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity to retrieve.</typeparam>
  /// <param name="ksql">The KSQL statement representing the pull query.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
  /// <returns>A ValueTask representing the asynchronous operation. The result is the retrieved entity.</returns>
  ValueTask<TEntity?> ExecutePullQuery<TEntity>(string ksql, CancellationToken cancellationToken = default);

  /// <summary>
  /// Add entity for insertion. In order to save them call SaveChangesAsync.
  /// </summary>
  /// <typeparam name="T">Type of entity to add.</typeparam>
  /// <param name="insertValues">Configurable insert values.</param>
  /// <param name="insertProperties">Optional insert properties.</param>
  void Add<T>(InsertValues<T> insertValues, InsertProperties? insertProperties = null);

  /// <summary>
  /// Add entity for insertion. In order to save them call SaveChangesAsync.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity">Entity to add</param>
  /// <param name="insertProperties">Optional insert properties.</param>
  void Add<T>(T entity, InsertProperties? insertProperties = null);

  /// <summary>
  /// Save the entities added to context.
  /// </summary>
  /// <returns>Save response.</returns>
  Task<HttpResponseMessage?> SaveChangesAsync(CancellationToken cancellationToken = default);
}
