using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal class KStreamSetDependencies : IKStreamSetDependencies
{
  public KStreamSetDependencies(IKSqlQbservableProvider provider, IKSqlDbProvider ksqlDbProvider, IKSqlQueryGenerator queryGenerator, IKSqlDbParameters queryStreamParameters)
  {
    Provider = provider;
    KsqlDBProvider = ksqlDbProvider;
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
