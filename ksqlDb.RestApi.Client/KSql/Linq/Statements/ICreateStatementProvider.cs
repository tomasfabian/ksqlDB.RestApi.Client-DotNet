using System.Linq.Expressions;

namespace ksqlDB.RestApi.Client.KSql.Linq.Statements
{
  public interface ICreateStatementProvider
  {
    ICreateStatement<TResult> CreateStatement<TResult>(Expression expression);
  }
}