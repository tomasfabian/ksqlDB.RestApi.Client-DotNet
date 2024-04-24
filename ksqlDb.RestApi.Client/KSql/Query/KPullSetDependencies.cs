using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query
{
  internal class KPullSetDependencies(
    IKSqlQbservableProvider provider,
    IKSqlDbProvider ksqlDbProvider,
    IKSqlQueryGenerator queryGenerator,
    IKSqlDbParameters queryParameters)
    : KSetDependencies(provider, ksqlDbProvider, queryGenerator, queryParameters), IKPullSetDependencies;
}
