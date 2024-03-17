using System.Linq.Expressions;

namespace ksqlDB.RestApi.Client.KSql.Linq.PullQueries;

/// <summary>
/// Execute a pull query by sending an HTTP request to the ksqlDB REST API, and the API responds with a single response.
/// </summary>
public interface IPullable
{
  /// <summary>
  /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of IPullable is executed.
  /// </summary>
  Type ElementType { get; }

  /// <summary>
  /// Gets the expression tree that is associated with the instance of IPullable.
  /// </summary>
  Expression Expression { get; }

  /// <summary>
  /// Gets the query provider that is associated with this data source.
  /// </summary>
  IPullQueryProvider Provider { get; }
}

/// <summary>
/// Execute a pull query by sending an HTTP request to the ksqlDB REST API, and the API responds with a single response.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPullable<T> : IPullable
{ 
  /// <summary>
  /// Pulls the current value from the materialized table and terminates. 
  /// </summary>
  /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
  /// <returns></returns>
  ValueTask<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    
  /// <summary>
  /// Pulls all values from the materialized view asynchronously and terminates. 
  /// </summary>
  IAsyncEnumerable<T> GetManyAsync(CancellationToken cancellationToken = default);
}
