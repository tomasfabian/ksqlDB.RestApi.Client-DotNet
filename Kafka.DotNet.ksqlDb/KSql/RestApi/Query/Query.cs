using System.Collections.Generic;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Query
{
  public record Query<T>
  {
    public string QueryId { get; set; }

    public IAsyncEnumerable<T> EnumerableQuery { get; set; }
  }
}