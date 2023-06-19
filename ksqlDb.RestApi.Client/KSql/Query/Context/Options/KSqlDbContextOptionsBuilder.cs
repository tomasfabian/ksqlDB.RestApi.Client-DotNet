using System.Text.Json;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.Options;

/// <summary>
/// KSqlDbContextOptionsBuilder provides a fluent API that allows you to configure various aspects of the `ksqlDB` context, such as the connection string, processing guarantee, and other options.
/// </summary>
public class KSqlDbContextOptionsBuilder : ISetupParameters
{
  private readonly ServiceCollection serviceCollection = new();

  public ISetupParameters UseKSqlDb(string url)
  {
    if (string.IsNullOrEmpty(url))
      throw new ArgumentNullException(nameof(url));

    Url = url;

    return this;
  }

  private string Url { get; set; }

  /// <summary>
  /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
  /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
  /// </summary>
  /// <typeparam name="TClient">The specified type of the typed client will be registered as a transient service in the service collection.</typeparam>
  /// <typeparam name="TImplementation">
  /// The implementation type of the typed client.</typeparam>
  /// <param name="configureClient">A delegate that intercepts the creation of an instance of <typeparamref name="TClient"/>.</param>
  /// <returns>An <see cref="IHttpClientBuilder"/> that can be utilized to configure the client.</returns>
  public IHttpClientBuilder ReplaceHttpClient<TClient, TImplementation>(Action<HttpClient> configureClient)
    where TClient : class
    where TImplementation : class, TClient
  {
    void OuterConfigureClient(HttpClient httpClient)
    {
      httpClient.BaseAddress = new Uri(Url);

#if !NETSTANDARD
      httpClient.DefaultRequestVersion = new Version(2, 0);
#endif

      configureClient(httpClient);
    }

    return serviceCollection.AddHttpClient<TClient, TImplementation>(OuterConfigureClient);
  }

#if !NETSTANDARD
  ISetupParameters ISetupParameters.SetupQueryStream(Action<IKSqlDbParameters> configure)
  {
    configure(InternalOptions.QueryStreamParameters);

    return this;
  }

#endif

  /// <summary>
  /// allows you to set basic authentication credentials for an HTTP client. 
  /// </summary>
  /// <param name="username">User name</param>
  /// <param name="password">Password</param>
  /// <returns>Returns this instance.</returns>
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
  /// <returns>The original ksqlDB context options builder</returns>
  ISetupParameters ISetupParameters.SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction)
  {
    optionsAction(jsonSerializerOptions);

    return this;
  }

  /// <summary>
  /// Allows you to configure ksqlDB query parameters such as processing guarantee or 'auto.offset.reset'.
  /// </summary>
  /// <param name="configure">A delegate that intercepts the creation of an instance of <typeparamref name="IKSqlDbParameters"/>.</param>
  /// <returns>Setup parameters</returns>
  ISetupParameters ISetupParameters.SetupQuery(Action<IKSqlDbParameters> configure)
  {
    configure(InternalOptions.QueryParameters);

    return this;
  }

  /// <summary>
  /// Allows you to configure the processing.guarantee streams property.
  /// </summary>
  /// <param name="processingGuarantee">Type of processing guarantee. exactly_once_v2 or at_least_once semantics</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters ISetupParameters.SetProcessingGuarantee(ProcessingGuarantee processingGuarantee)
  {
    InternalOptions.SetProcessingGuarantee(processingGuarantee);

    return this;
  }

  /// <summary>
  /// Allows you to configure the auto.offset.reset streams property. 
  /// </summary>
  /// <param name="autoOffsetReset"></param>
  /// <returns></returns>
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
        JsonSerializerOptions = jsonSerializerOptions,
        ServiceCollection = serviceCollection
      };
    }
  }
}
