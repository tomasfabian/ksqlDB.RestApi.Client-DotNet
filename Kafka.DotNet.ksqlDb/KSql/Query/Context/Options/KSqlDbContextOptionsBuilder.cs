using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context.Options
{
  public class KSqlDbContextOptionsBuilder : ISetupParameters
  {
    public ISetupParameters UseKSqlDb(string url)
    {
      if(string.IsNullOrEmpty(url))
        throw new ArgumentNullException(nameof(url));

      Url = url;

      return this;
    }

    private string Url { get; set; }

#if !NETSTANDARD
    ISetupParameters ISetupParameters.SetupQueryStream(Action<IQueryOptions> configure)
    {
      configure(InternalOptions.QueryStreamParameters);

      return this;
    }

#endif

    ISetupParameters ISetupParameters.SetupQuery(Action<IQueryOptions> configure)
    {
      configure(InternalOptions.QueryParameters);

      return this;
    }

    ISetupParameters ISetupParameters.SetProcessingGuarantee(ProcessingGuarantee processingGuarantee)
    {
      InternalOptions.SetProcessingGuarantee(processingGuarantee);

      return this;
    }

    ISetupParameters ISetupParameters.SetAutoOffsetReset(AutoOffsetReset autoOffsetReset)
    {
      InternalOptions.SetAutoOffsetReset(autoOffsetReset);

      return this;
    }

    private KSqlDBContextOptions contextOptions;

    KSqlDBContextOptions ICreateOptions.Options => InternalOptions;

    internal KSqlDBContextOptions InternalOptions
    {
      get { return contextOptions ??= new KSqlDBContextOptions(Url); }
    }
  }
}