using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal interface IKStreamSetDependencies
{
  IKSqlDbProvider KsqlDBProvider { get; }
  IKSqlQueryGenerator KSqlQueryGenerator { get; }
  IKSqlDbParameters QueryStreamParameters { get; }
}