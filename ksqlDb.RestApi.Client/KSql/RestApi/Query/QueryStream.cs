using System.Collections.Generic;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query
{
  public record QueryStream<T>
  {
    public string QueryId { get; set; }

    public IAsyncEnumerable<T> EnumerableQuery { get; set; }
  }
}