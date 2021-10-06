using Kafka.DotNet.ksqlDB.KSql.Query.Options;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  public interface IKSqlDbParameters : IQueryParameters
  {
    AutoOffsetReset AutoOffsetReset { get; set; }

    IKSqlDbParameters Clone();
  }
}