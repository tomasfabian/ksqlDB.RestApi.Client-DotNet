using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries
{
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

  public interface IPullable<T> : IPullable
  { 
    ValueTask<T> GetAsync(CancellationToken cancellationToken = default);
  }
}