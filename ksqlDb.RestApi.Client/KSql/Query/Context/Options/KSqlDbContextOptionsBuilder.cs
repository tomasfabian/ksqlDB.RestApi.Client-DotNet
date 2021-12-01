using System;
using System.Text.Json;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.Options
{
  public class KSqlDbContextOptionsBuilder : ISetupParameters
  {
    public ISetupParameters UseKSqlDb(string url)
    {
      if (string.IsNullOrEmpty(url))
        throw new ArgumentNullException(nameof(url));

      Url = url;

      return this;
    }

    private string Url { get; set; }

#if !NETSTANDARD
    ISetupParameters ISetupParameters.SetupQueryStream(Action<IKSqlDbParameters> configure)
    {
      configure(InternalOptions.QueryStreamParameters);

      return this;
    }

#endif

    ISetupParameters ISetupParameters.SetBasicAuthCredentials(string username, string password)
    {
      InternalOptions.SetBasicAuthCredentials(username, password);

      return this;
    }

    private readonly JsonSerializerOptions jsonSerializerOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    /// <summary>
    /// Interception of JsonSerializerOptions.
    /// </summary>
    /// <param name="optionsAction">Action to configure the JsonSerializerOptions for the materialization of the incoming values.</param>
    /// <returns>The original KSqlDb context options builder</returns>
    ISetupParameters ISetupParameters.SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction)
    {
      optionsAction(jsonSerializerOptions);

      return this;
    }

    ISetupParameters ISetupParameters.SetupQuery(Action<IKSqlDbParameters> configure)
    {
      configure(InternalOptions.QueryParameters);

      return this;
    }

    /// <summary>
    /// Enable exactly-once or at_least_once semantics
    /// </summary>
    /// <param name="processingGuarantee">Type of processing guarantee.</param>
    /// <returns>Returns this instance.</returns>
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
      get
      {
        return contextOptions ??= new KSqlDBContextOptions(Url)
        {
          JsonSerializerOptions = jsonSerializerOptions
        };
      }
    }
  }
}