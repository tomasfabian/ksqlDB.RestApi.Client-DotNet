using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters
{
  public interface IPushQueryParameters : IKSqlDbParameters
  {
    AutoOffsetReset AutoOffsetReset { get; set; }
  }
}
