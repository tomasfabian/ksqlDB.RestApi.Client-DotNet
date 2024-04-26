using System.Text.Json;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
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

  private string? Url { get; set; }

  /// <summary>
  /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
  /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
  /// </summary>
  /// <typeparam name="TClient">The specified type of the typed client will be registered as a transient service in the service collection.</typeparam>
  /// <typeparam name="TImplementation">
  /// The implementation type of the typed client.</typeparam>
  /// <param name="configureClient">A delegate that intercepts the creation of an instance of <typeparamref name="TClient"/>.</param>
  /// <returns>A <see cref="IHttpClientBuilder"/> that can be utilized to configure the client.</returns>
  public IHttpClientBuilder ReplaceHttpClient<TClient, TImplementation>(Action<HttpClient> configureClient)
    where TClient : class
    where TImplementation : class, TClient
  {
    void OuterConfigureClient(HttpClient httpClient)
    {
      httpClient.BaseAddress = new Uri(Url ?? throw new InvalidOperationException());

#if !NETSTANDARD
      httpClient.DefaultRequestVersion = new Version(2, 0);
#endif

      configureClient(httpClient);
    }

    return serviceCollection.AddHttpClient<TClient, TImplementation>(OuterConfigureClient!);
  }

  /// <summary>
  /// Sets the KSQL query endpoints when using pull and push queries.
  /// </summary>
  ISetupParameters ISetupParameters.SetEndpointType(EndpointType endpointType)
  {
    InternalOptions.EndpointType = endpointType;

    return this;
  }

  /// <summary>
  /// Configures the parameters for setting up a push query for the 'query-stream' endpoint.
  /// Allows you to configure ksqlDB query parameters such as processing guarantee or 'auto.offset.reset'.
  /// </summary>
  /// <param name="configure">A delegate that intercepts the creation of an instance of <see cref="IPushQueryParameters"/>.</param>
  /// <returns>Setup parameters</returns>
  ISetupParameters ISetupParameters.SetupQuery(Action<IPushQueryParameters> configure)
  {
    configure(InternalOptions.QueryParameters);

    return this;
  }

#if !NETSTANDARD
  /// <summary>
  /// Configures the parameters for setting up a push query for the 'query-stream' endpoint.
  /// Allows you to configure ksqlDB query parameters such as processing guarantee or 'auto.offset.reset'.
  /// </summary>
  /// <param name="configure">An action to configure the parameters using <see cref="IPushQueryParameters"/>.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters ISetupParameters.SetupQueryStream(Action<IPushQueryParameters> configure)
  {
    configure(InternalOptions.QueryStreamParameters);

    return this;
  }
#endif

  /// <summary>
  /// Configures the parameters for setting up a pull query.
  /// </summary>
  /// <param name="configure">An action to configure the parameters using <see cref="IPullQueryParameters"/>.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters ISetupParameters.SetupPullQuery(Action<IPullQueryParameters> configure)
  {
    configure(InternalOptions.PullQueryParameters);

    return this;
  }

  /// <summary>
  /// Allows you to set basic authentication credentials for an HTTP client. 
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
  /// Allows you to set escaping for identifiers
  /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
  /// </summary>
  /// <param name="escaping">Escaping mode</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters ISetupParameters.SetIdentifierEscaping(IdentifierEscaping escaping)
  {
    InternalOptions.IdentifierEscaping = escaping;

    return this;
  }

  /// <summary>
  /// Allows you to configure the 'processing.guarantee' streams property.
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
  /// <param name="autoOffsetReset">The auto offset reset value to set.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters ISetupParameters.SetAutoOffsetReset(AutoOffsetReset autoOffsetReset)
  {
    InternalOptions.SetAutoOffsetReset(autoOffsetReset);

    return this;
  }

  private KSqlDBContextOptions? contextOptions;

  KSqlDBContextOptions ICreateOptions.Options => InternalOptions;

  internal KSqlDBContextOptions InternalOptions
  {
    get
    {
      return contextOptions ??= new KSqlDBContextOptions(Url ?? throw new InvalidOperationException())
      {
        JsonSerializerOptions = jsonSerializerOptions,
        ServiceCollection = serviceCollection
      };
    }
  }
}
