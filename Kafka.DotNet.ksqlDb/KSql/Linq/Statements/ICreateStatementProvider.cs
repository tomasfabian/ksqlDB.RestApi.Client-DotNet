using System.Linq.Expressions;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.Statements
{
  public interface ICreateStatementProvider
  {
    ICreateStatement<TResult> CreateStatement<TResult>(Expression expression);
  }
}