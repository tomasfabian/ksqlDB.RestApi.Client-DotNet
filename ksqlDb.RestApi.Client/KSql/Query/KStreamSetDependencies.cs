using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal class KStreamSetDependencies : IKStreamSetDependencies
{
  private readonly IKSqlDbParameters queryStreamParameters;

  public KStreamSetDependencies(IKSqlQbservableProvider provider, IKSqlDbProvider ksqlDbProvider, IKSqlQueryGenerator queryGenerator, IKSqlDbParameters queryStreamParameters)
  {
    Provider = provider;
    KsqlDBProvider = ksqlDbProvider;
    KSqlQueryGenerator = queryGenerator;
    this.queryStreamParameters = queryStreamParameters;

    QueryContext = new QueryContext();
  }

  public IKSqlQbservableProvider Provider { get; }

  public IKSqlDbProvider KsqlDBProvider { get; }

  public IKSqlQueryGenerator KSqlQueryGenerator { get; }

  public IKSqlDbParameters QueryStreamParameters
  {
    get { return queryStreamParameters.Clone(); }
  }

  public QueryContext QueryContext { get; }
}
