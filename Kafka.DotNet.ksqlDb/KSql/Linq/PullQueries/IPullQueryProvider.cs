using System.Linq.Expressions;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries
{
  public interface IPullQueryProvider
  {
    IPullable<TResult> CreateQuery<TResult>(Expression expression);
  }
}