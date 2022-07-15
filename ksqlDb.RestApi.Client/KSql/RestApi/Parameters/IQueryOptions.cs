using System.Collections.Generic;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public interface IQueryOptions
{
  Dictionary<string, string> Properties { get; }
}