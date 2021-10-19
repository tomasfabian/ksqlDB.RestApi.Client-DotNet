using System.Linq.Expressions;

namespace ksqlDB.RestApi.Client.KSql.Linq.PullQueries
{
  public interface IPullQueryProvider
  {
    IPullable<TResult> CreateQuery<TResult>(Expression expression);
  }
}