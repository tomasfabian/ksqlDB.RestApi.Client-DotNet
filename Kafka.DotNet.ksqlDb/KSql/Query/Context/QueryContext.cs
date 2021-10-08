using System.Collections.Generic;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  public class QueryContext
  {
    public string FromItemName { get; internal set; }
    internal AutoOffsetReset? AutoOffsetReset { get; set; }
    internal BasicAuthCredentials Credentials { get; set; }
  }
}