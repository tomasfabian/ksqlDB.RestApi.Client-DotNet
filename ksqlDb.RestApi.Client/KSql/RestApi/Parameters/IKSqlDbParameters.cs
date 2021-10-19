using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters
{
  public interface IKSqlDbParameters : IQueryParameters
  {
    AutoOffsetReset AutoOffsetReset { get; set; }

    IKSqlDbParameters Clone();
  }
}