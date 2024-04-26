using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query
{
  internal class KSetDependencies : IKSetDependencies
  {
    private readonly IKSqlDbParameters queryParameters;

    public KSetDependencies(IKSqlQbservableProvider provider, IKSqlDbProvider ksqlDbProvider, IKSqlQueryGenerator queryGenerator, IKSqlDbParameters queryParameters)
    {
      Provider = provider;
      KSqlDbProvider = ksqlDbProvider;
      KSqlQueryGenerator = queryGenerator;
      this.queryParameters = queryParameters;

      QueryContext = new QueryContext();
    }

    public IKSqlQbservableProvider Provider { get; }

    public IKSqlDbProvider KSqlDbProvider { get; }

    public IKSqlQueryGenerator KSqlQueryGenerator { get; }

    public IKSqlDbParameters QueryStreamParameters
    {
      get { return queryParameters.Clone(); }
    }

    public QueryContext QueryContext { get; }
  }
}
