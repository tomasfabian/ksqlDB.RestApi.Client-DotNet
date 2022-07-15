namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public interface IQueryParameters : IQueryOptions
{
  string Sql { get; set; }
    
  string this[string key] { get; set; }
}