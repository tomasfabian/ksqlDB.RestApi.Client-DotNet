namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public interface IKSqlDbParameters : IQueryParameters
{
  IKSqlDbParameters Clone();
}
