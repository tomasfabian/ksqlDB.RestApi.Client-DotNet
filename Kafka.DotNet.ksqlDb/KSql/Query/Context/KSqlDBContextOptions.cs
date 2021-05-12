using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  public sealed class KSqlDBContextOptions
  {
    public KSqlDBContextOptions(string url)
    {
      if(string.IsNullOrEmpty(url))
        throw new ArgumentNullException(nameof(url));

      Url = url;

      QueryParameters ??= new QueryParameters
      {
        [RestApi.Parameters.QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower()
      };

      QueryStreamParameters ??= new QueryStreamParameters
      {
        [QueryStreamParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower()
      };
    }

    public bool ShouldPluralizeStreamName { get; set; } = true;

    public string Url { get; }

    public QueryStreamParameters QueryStreamParameters { get; internal set; }

    public IQueryParameters QueryParameters { get; internal set; }
  }
}