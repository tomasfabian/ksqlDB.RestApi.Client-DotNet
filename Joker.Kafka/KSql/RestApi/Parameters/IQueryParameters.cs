namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  public interface IQueryParameters : IQueryOptions
  {
    string Sql { get; set; }
  }
}