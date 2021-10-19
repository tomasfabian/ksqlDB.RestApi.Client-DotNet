using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace ksqlDB.RestApi.Client.KSql.Query
{
  public interface IKSqlQueryGenerator
  {
    bool ShouldEmitChanges { get; set; }

    string BuildKSql(Expression expression, QueryContext queryContext);
  }
}