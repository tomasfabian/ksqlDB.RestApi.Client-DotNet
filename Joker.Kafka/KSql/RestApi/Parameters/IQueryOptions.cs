using System.Collections.Generic;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  public interface IQueryOptions
  {
    Dictionary<string, string> Properties { get; }
  }
}