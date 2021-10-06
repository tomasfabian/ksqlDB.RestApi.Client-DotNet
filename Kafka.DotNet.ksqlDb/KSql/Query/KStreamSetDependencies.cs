using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal class KStreamSetDependencies : IKStreamSetDependencies
  {
    public KStreamSetDependencies(IKSqlQbservableProvider provider, IKSqlDbProvider ksqlDBProvider, IKSqlQueryGenerator queryGenerator, IKSqlDbParameters queryStreamParameters)
    {
      Provider = provider;
      KsqlDBProvider = ksqlDBProvider;
      KSqlQueryGenerator = queryGenerator;
      QueryStreamParameters = queryStreamParameters;

      QueryContext = new QueryContext();
    }

    public IKSqlQbservableProvider Provider { get; }

    public IKSqlDbProvider KsqlDBProvider { get; }

    public IKSqlQueryGenerator KSqlQueryGenerator { get; }

    public IKSqlDbParameters QueryStreamParameters { get; }

    public QueryContext QueryContext { get; }
  }
}