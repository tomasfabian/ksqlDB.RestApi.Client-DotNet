using System;
using Kafka.DotNet.ksqlDB.KSql.Config;
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
        [QueryStreamParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower(),
      };
    }

    public bool ShouldPluralizeFromItemName { get; set; } = true;

    public string Url { get; }

    public QueryStreamParameters QueryStreamParameters { get; internal set; }

    public IKSqlDbParameters QueryParameters { get; internal set; }

    public void SetProcessingGuarantee(ProcessingGuarantee processingGuarantee)
    {
      string guarantee = processingGuarantee switch
      {
        ProcessingGuarantee.AtLeastOnce => "at_least_once",
        ProcessingGuarantee.ExactlyOnce => "exactly_once",
        _ => throw new ArgumentOutOfRangeException(nameof(processingGuarantee), processingGuarantee, null)
      };

      QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee] = guarantee; 
      QueryParameters[KSqlDbConfigs.ProcessingGuarantee] = guarantee;
    }

    public void SetAutoOffsetReset(AutoOffsetReset autoOffsetReset)
    {
      QueryParameters[RestApi.Parameters.QueryParameters.AutoOffsetResetPropertyName] =
        autoOffsetReset.ToString().ToLower();

      QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName] =
        autoOffsetReset.ToString().ToLower();
    }

    internal KSqlDBContextOptions Clone()
    {
      var options = new KSqlDBContextOptions(Url)
      {
        ShouldPluralizeFromItemName = ShouldPluralizeFromItemName,
        QueryParameters = ((QueryParameters) QueryParameters).Clone(),
        QueryStreamParameters = QueryStreamParameters.Clone() as QueryStreamParameters
      };

      return options;
    }
  }
}