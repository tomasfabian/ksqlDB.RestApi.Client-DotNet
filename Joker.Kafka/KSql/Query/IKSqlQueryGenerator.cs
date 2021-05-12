using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  public interface IKSqlQueryGenerator
  {
    bool ShouldEmitChanges { get; set; }

    string BuildKSql(Expression expression, QueryContext queryContext);
  }
}