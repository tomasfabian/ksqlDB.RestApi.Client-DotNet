using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal interface IKStreamSetDependencies
  {
    IKSqlDbProvider KsqlDBProvider { get; }
    IKSqlQueryGenerator KSqlQueryGenerator { get; }
    IKSqlDbParameters QueryStreamParameters { get; }
  }
}