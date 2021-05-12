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
      var queryStreamParameters = CreateQueryStreamParameters();
      InternalOptions.QueryStreamParameters = queryStreamParameters;

      configure(queryStreamParameters);

      return this;
    }

    private QueryStreamParameters CreateQueryStreamParameters()
    {
      return new()
      {
        [QueryStreamParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower()
      };
    }
#endif

    ISetupParameters ISetupParameters.SetupQuery(Action<IQueryOptions> configure)
    {
      var queryParameters = CreateQueryParameters();

      InternalOptions.QueryParameters = queryParameters;
      
      configure(queryParameters);

      return this;
    }

    private QueryParameters CreateQueryParameters()
    {
      return new()
      {
        [QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower()
      };
    }

    private KSqlDBContextOptions contextOptions;

    KSqlDBContextOptions ICreateOptions.Options => InternalOptions;

    internal KSqlDBContextOptions InternalOptions
    {
      get { return contextOptions ??= new KSqlDBContextOptions(Url); }
    }
  }
}