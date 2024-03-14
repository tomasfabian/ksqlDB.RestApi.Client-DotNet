using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ksqlDb.RestApi.Client.Infrastructure.Logging;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDb.RestApi.Client.KSql.RestApi.Responses.Asserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

public class KSqlDbRestApiClient : IKSqlDbRestApiClient
{
  private readonly EntityProvider entityProvider = new();
  private readonly IHttpClientFactory httpClientFactory;
  private readonly ILogger? logger;

  public KSqlDbRestApiClient(IHttpClientFactory httpClientFactory, ILoggerFactory? loggerFactory = null)
  {
    this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    if (loggerFactory != null)
      logger = loggerFactory.CreateLogger(LoggingCategory.Name);
  }

  public bool DisposeHttpClient { get; set; }

  internal static readonly string MediaType = "application/vnd.ksql.v1+json";

  private static string NullOrWhiteSpaceErrorMessage => "Can't be null, empty, or contain only whitespace.";

  private BasicAuthCredentials? basicAuthCredentials;

  /// <summary>
  /// Sets Basic HTTP authentication mechanism.
  /// </summary>
  /// <param name="credentials">User credentials.</param>
  /// <returns>This instance.</returns>
  public IKSqlDbRestApiClient SetCredentials(BasicAuthCredentials credentials)
  {
    basicAuthCredentials = credentials;

    return this;
  }

  /// <summary>
  /// Run a sequence of SQL statements.
  /// </summary>
  /// <param name="ksqlDbStatement">The text of the SQL statements.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
  {
    logger?.LogInformation("Executing command: {SQL}", ksqlDbStatement.Sql);

    return ExecuteStatementAsync(ksqlDbStatement, ksqlDbStatement.EndpointType, ksqlDbStatement.ContentEncoding, cancellationToken);
  }

  private async Task<HttpResponseMessage> ExecuteStatementAsync(object content, EndpointType endPointType, Encoding encoding, CancellationToken cancellationToken = default)
  {
    var httpClient = httpClientFactory.CreateClient();

    var httpRequestMessage = CreateHttpRequestMessage(content, endPointType, encoding);

    httpClient.DefaultRequestHeaders.Accept.Add(
      new MediaTypeWithQualityHeaderValue(MediaType));

    var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken)
      .ConfigureAwait(false);

    if (logger != null)
    {
      string response = await httpResponseMessage.Content
#if NETSTANDARD
        .ReadAsStringAsync();
#else
        .ReadAsStringAsync(cancellationToken);
#endif
      logger?.LogDebug("Command response ({StatusCode}): {Response}", httpResponseMessage.StatusCode, response);
    }

    if (DisposeHttpClient)
      httpClient.Dispose();

    return httpResponseMessage;
  }

  internal HttpRequestMessage CreateHttpRequestMessage(KSqlDbStatement ksqlDbStatement)
  {
    return CreateHttpRequestMessage(ksqlDbStatement, ksqlDbStatement.EndpointType, ksqlDbStatement.ContentEncoding);
  }

  internal HttpRequestMessage CreateHttpRequestMessage(object content, EndpointType endPointType, Encoding encoding)
  {
    var data = CreateContent(content, encoding);

    var endpoint = GetEndpoint(endPointType);

    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
    {
      Content = data
    };

    if (basicAuthCredentials != null)
    {
      string basicAuthHeader = basicAuthCredentials.CreateToken();

      httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(basicAuthCredentials.Schema, basicAuthHeader);
    }

    return httpRequestMessage;
  }

  internal static StringContent CreateContent(KSqlDbStatement ksqlDbStatement)
  {
    return CreateContent(ksqlDbStatement, ksqlDbStatement.ContentEncoding);
  }

  internal static StringContent CreateContent(object content, Encoding encoding)
  {
    var json = JsonSerializer.Serialize(content);

    var data = new StringContent(json, encoding, MediaType);

    return data;
  }

  internal static string GetEndpoint(KSqlDbStatement ksqlDbStatement)
  {
    return GetEndpoint(ksqlDbStatement.EndpointType);
  }

  internal static string GetEndpoint(EndpointType endpointType)
  {
    var endpoint = endpointType switch
    {
      EndpointType.KSql => "/ksql",
      EndpointType.Query => "/query",
      EndpointType.CloseQuery => "/close-query",
      _ => throw new ArgumentOutOfRangeException(nameof(endpointType), $"Unknown '{nameof(EndpointType)}' type {endpointType}.")
    };

    return endpoint;
  }

  #region Creation

  /// <summary>
  /// Create a new stream with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a stream with the same name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    var ksql = StatementGenerator.CreateStream<T>(creationMetadata, ifNotExists);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Create a new stream or replace an existing one with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateOrReplaceStreamAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    var ksql = StatementGenerator.CreateOrReplaceStream<T>(creationMetadata);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Create a new read-only source stream with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a stream with the same name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateSourceStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    creationMetadata.IsReadOnly = true;

    var ksql = StatementGenerator.CreateStream<T>(creationMetadata, ifNotExists);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Create a new table with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a table with the same name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateTableAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    var ksql = StatementGenerator.CreateTable<T>(creationMetadata, ifNotExists);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Create a new read-only table with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a table with the same name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateSourceTableAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    creationMetadata.IsReadOnly = true;

    var ksql = StatementGenerator.CreateTable<T>(creationMetadata, ifNotExists);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Create a new table or replace an existing one with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateOrReplaceTableAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));
#else
    ArgumentNullException.ThrowIfNull(creationMetadata);
#endif

    var ksql = StatementGenerator.CreateOrReplaceTable<T>(creationMetadata);

    return ExecuteAsync(ksql, cancellationToken);
  }

  private Task<HttpResponseMessage> ExecuteAsync(string ksql, CancellationToken cancellationToken = default)
  {
    var ksqlStatement = new KSqlDbStatement(ksql);

    return ExecuteStatementAsync(ksqlStatement, cancellationToken);
  }

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateTypeAsync<T>(CancellationToken cancellationToken = default)
  {
    var properties = new TypeProperties();
    return CreateTypeAsync<T>(properties, cancellationToken);
  }

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="typeName">Optional name of the type. Otherwise, the type name is inferred from the generic type name.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateTypeAsync<T>(string typeName, CancellationToken cancellationToken = default)
  {
    var properties = new TypeProperties { EntityName = typeName };
    return CreateTypeAsync<T>(properties, cancellationToken);
  }

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="properties">Type configuration</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> CreateTypeAsync<T>(TypeProperties properties, CancellationToken cancellationToken = default)
  {
    var ksql = new TypeGenerator().Print<T>(properties);

    return ExecuteAsync(ksql, cancellationToken);
  }

  /// <summary>
  /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions.
  /// </summary>
  /// <param name="dropTypeProperties">Configuration for dropping the type.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> DropTypeAsync<T>(DropTypeProperties dropTypeProperties, CancellationToken cancellationToken = default)
  {
    var typeName = entityProvider.GetFormattedName<T>(dropTypeProperties);
    string dropStatement = StatementTemplates.DropType(typeName);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions.
  /// </summary>
  /// <param name="typeName">Name of the type to remove.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> DropTypeAsync(string typeName, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropType(typeName);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions. The statement doesn't fail if the type doesn't exist.
  /// </summary>
  /// <param name="typeName">Name of the type to remove.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> DropTypeIfExistsAsync(string typeName, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropTypeIfExists(typeName);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  #endregion

  /// <summary>
  /// Produce a row into an existing stream or table and its underlying topic based on explicitly specified entity properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity">Entity for insertion.</param>
  /// <param name="insertProperties">Overrides conventions.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  public Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties? insertProperties = null, CancellationToken cancellationToken = default)
  {
    var insertStatement = ToInsertStatement(entity, insertProperties);

    var httpResponseMessage = ExecuteStatementAsync(insertStatement, cancellationToken);

    return httpResponseMessage;
  }

  /// <summary>
  /// Generates raw 'Insert Into' string, but does not execute it.
  /// </summary>
  /// <typeparam name="T">Type of entity</typeparam>
  /// <param name="insertValues">Insert values</param>
  /// <param name="insertProperties">Insert configuration</param>
  /// <returns>A <see cref="KSqlDbStatement"/></returns>
  public KSqlDbStatement ToInsertStatement<T>(InsertValues<T> insertValues, InsertProperties? insertProperties = null)
  {
    var insertStatement = new CreateInsert().Generate(insertValues, insertProperties);

    return new KSqlDbStatement(insertStatement);
  }

  /// <summary>
  /// Generates raw 'Insert Into' string, but does not execute it.
  /// </summary>
  /// <typeparam name="T">Type of entity</typeparam>
  /// <param name="entity">Entity for insertion.</param>
  /// <param name="insertProperties">Overrides conventions.</param>
  /// <returns>A <see cref="KSqlDbStatement"/></returns>
  public KSqlDbStatement ToInsertStatement<T>(T entity, InsertProperties? insertProperties = null)
  {
    var insertStatement = new CreateInsert().Generate(entity, insertProperties);

    return new KSqlDbStatement(insertStatement);
  }

  #region Get

  /// <summary>
  /// List the defined streams.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<StreamsResponse[]> GetStreamsAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowStreams;

    return ExecuteStatementAsync<StreamsResponse>(showStatement, cancellationToken);
  }

  /// <summary>
  /// List the defined tables.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public async Task<TablesResponse[]> GetTablesAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowTables;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToTablesResponseAsync().ConfigureAwait(false);

    return responses;
  }

  #region Topics

  //SHOW | LIST [ALL] TOPICS [EXTENDED];

  /// <summary>
  /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>List of topics.</returns>
  public async Task<TopicsResponse[]> GetTopicsAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowTopics;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToTopicsResponseAsync().ConfigureAwait(false);

    return responses;
  }

  /// <summary>
  /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to, including hidden topics.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>List of all topics.</returns>
  public async Task<TopicsResponse[]> GetAllTopicsAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowAllTopics;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToTopicsResponseAsync().ConfigureAwait(false);

    return responses;
  }

  /// <summary>
  /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>List of topics. Also displays consumer groups and their active consumer counts.</returns>
  public async Task<TopicsExtendedResponse[]> GetTopicsExtendedAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowTopicsExtended;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToTopicsExtendedResponseAsync().ConfigureAwait(false);

    return responses;
  }

  /// <summary>
  /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to, including hidden topics.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>List of all topics. Also displays consumer groups and their active consumer counts.</returns>
  public async Task<TopicsExtendedResponse[]> GetAllTopicsExtendedAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowAllTopicsExtended;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToTopicsExtendedResponseAsync().ConfigureAwait(false);

    return responses;
  }

  #endregion

  #region GetQueriesAsync

  /// <summary>
  /// Lists queries running in the cluster.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>List of queries.</returns>
  public async Task<QueriesResponse[]> GetQueriesAsync(CancellationToken cancellationToken = default)
  {
    //SHOW | LIST QUERIES [EXTENDED];
    string showStatement = StatementTemplates.ShowQueries;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var responses = await httpResponseMessage.ToQueriesResponseAsync().ConfigureAwait(false);

    return responses;
  }

  #endregion

  #endregion

  #region Connectors

  /// <summary>
  /// List all connectors in the Connect cluster.
  /// </summary>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public async Task<ConnectorsResponse[]> GetConnectorsAsync(CancellationToken cancellationToken = default)
  {
    string showStatement = StatementTemplates.ShowConnectors;

    KSqlDbStatement ksqlDbStatement = new(showStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    return await httpResponseMessage.ToConnectorsResponseAsync();
  }

  /// <summary>
  /// Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.
  /// </summary>
  /// <param name="config">Configuration passed into the WITH clause.</param>
  /// <param name="connectorName">Name of the connector to create.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> CreateSourceConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (config == null) throw new ArgumentNullException(nameof(config));
#else
    ArgumentNullException.ThrowIfNull(config);
#endif

    if (string.IsNullOrWhiteSpace(connectorName))
      throw new ArgumentException(NullOrWhiteSpaceErrorMessage, nameof(connectorName));

    string createConnectorStatement = config.ToCreateConnectorStatement(connectorName, ifNotExists);

    KSqlDbStatement ksqlDbStatement = new(createConnectorStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.
  /// </summary>
  /// <param name="config">Configuration passed into the WITH clause.</param>
  /// <param name="connectorName">Name of the connector to create.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> CreateSinkConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default)
  {
#if NETSTANDARD
    if (config == null) throw new ArgumentNullException(nameof(config));
#else
    ArgumentNullException.ThrowIfNull(config);
#endif

    if (string.IsNullOrWhiteSpace(connectorName))
      throw new ArgumentException(NullOrWhiteSpaceErrorMessage, nameof(connectorName));

    string createConnectorStatement = config.ToCreateConnectorStatement(connectorName, ifNotExists, ConnectorType.Sink);

    KSqlDbStatement ksqlDbStatement = new(createConnectorStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.
  /// </summary>
  /// <param name="connectorName">Name of the connector to drop.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropConnectorIfExistsAsync(string connectorName, CancellationToken cancellationToken = default)
  {
    string dropIfExistsStatement = StatementTemplates.DropConnectorIfExists(connectorName);

    KSqlDbStatement ksqlDbStatement = new(dropIfExistsStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
  /// </summary>
  /// <param name="connectorName">Name of the connector to drop.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropConnectorAsync(string connectorName, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropConnector(connectorName);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  #endregion

  /// <summary>
  /// Drops an existing stream.
  /// DROP STREAM [IF EXISTS] stream_name [DELETE TOPIC];
  /// </summary>
  /// <param name="dropFromItemProperties">Configuration for dropping the stream.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropStreamAsync<T>(DropFromItemProperties dropFromItemProperties, CancellationToken cancellationToken = default)
  {
    var streamName = entityProvider.GetFormattedName<T>(dropFromItemProperties);
    string dropStatement = StatementTemplates.DropStream(streamName, dropFromItemProperties.UseIfExistsClause, dropFromItemProperties.DeleteTopic);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drops an existing stream.
  /// DROP STREAM [IF EXISTS] stream_name [DELETE TOPIC];
  /// </summary>
  /// <param name="streamName">Name of the stream to delete.</param>
  /// <param name="useIfExistsClause">If the IF EXISTS clause is present, the statement doesn't fail if the stream doesn't exist.</param>
  /// <param name="deleteTopic">If the DELETE TOPIC clause is present, the stream's source topic is marked for deletion.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropStreamAsync(string streamName, bool useIfExistsClause, bool deleteTopic, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropStream(streamName, useIfExistsClause, deleteTopic);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drops an existing stream.
  /// DROP STREAM stream_name;
  /// </summary>
  /// <param name="streamName">Name of the stream to delete.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropStreamAsync(string streamName, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropStream(streamName, useIfExists: false, deleteTopic: false);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drops an existing table.
  /// DROP TABLE [IF EXISTS] table_name [DELETE TOPIC];
  /// </summary>
  /// <typeparam name="T">The type that represents the table.</typeparam>
  /// <param name="dropFromItemProperties">Configuration for dropping the table.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropTableAsync<T>(DropFromItemProperties dropFromItemProperties, CancellationToken cancellationToken = default)
  {
    var tableName = entityProvider.GetFormattedName<T>(dropFromItemProperties);
    string dropStatement = StatementTemplates.DropTable(tableName, dropFromItemProperties.UseIfExistsClause, dropFromItemProperties.DeleteTopic);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drops an existing table.
  /// DROP TABLE [IF EXISTS] table_name [DELETE TOPIC];
  /// </summary>
  /// <param name="tableName">Name of the table to delete.</param>
  /// <param name="useIfExistsClause">If the IF EXISTS clause is present, the statement doesn't fail if the table doesn't exist.</param>
  /// <param name="deleteTopic">If the DELETE TOPIC clause is present, the table's source topic is marked for deletion.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropTableAsync(string tableName, bool useIfExistsClause, bool deleteTopic, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropTable(tableName, useIfExistsClause, deleteTopic);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Drops an existing table.
  /// DROP TABLE table_name;
  /// </summary>
  /// <param name="tableName">Name of the table to delete.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  public Task<HttpResponseMessage> DropTableAsync(string tableName, CancellationToken cancellationToken = default)
  {
    string dropStatement = StatementTemplates.DropTable(tableName);

    KSqlDbStatement ksqlDbStatement = new(dropStatement);

    return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }

  /// <summary>
  /// Pause a persistent query.
  /// </summary>
  /// <param name="queryId">ID of the query to pause.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  public async Task<StatementResponse[]> PausePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default)
  {
    string pauseStatement = StatementTemplates.PausePersistentQuery(queryId);

    KSqlDbStatement ksqlDbStatement = new(pauseStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    return statementResponse;
  }

  /// <summary>
  /// Resume a paused persistent query.
  /// </summary>
  /// <param name="queryId">ID of the query to resume.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  public async Task<StatementResponse[]> ResumePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default)
  {
    string resumePersistentQuery = StatementTemplates.ResumePersistentQuery(queryId);

    KSqlDbStatement ksqlDbStatement = new(resumePersistentQuery);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    return statementResponse;
  }

  /// <summary>
  /// Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.
  /// </summary>
  /// <param name="queryId">ID of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  public async Task<StatementResponse[]> TerminatePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default)
  {
    string terminateStatement = StatementTemplates.TerminatePersistentQuery(queryId);

    KSqlDbStatement ksqlDbStatement = new(terminateStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    return statementResponse;
  }

  /// <summary>
  /// Terminate a push query.
  /// </summary>
  /// <param name="queryId">ID of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  public async Task<HttpResponseMessage> TerminatePushQueryAsync(string queryId, CancellationToken cancellationToken = default)
  {
    //https://github.com/confluentinc/ksql/issues/7559
    //"{"@type":"generic_error","error_code":50000,"message":"On wrong context or worker"}"

    var closeQuery = new CloseQuery
    {
      QueryId = queryId
    };

    var response = await ExecuteStatementAsync(closeQuery, EndpointType.CloseQuery, Encoding.UTF8, cancellationToken);

    return response;
  }

  /// <summary>
  /// Show the execution plan of a running query, show the execution plan plus additional runtime information and metrics.
  /// </summary>
  /// <param name="queryId">ID of the query to explain.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
  /// <returns>ExplainResponse with execution plan plus additional runtime information and metrics.</returns>
  private Task<ExplainResponse[]> ExplainAsync(string queryId, CancellationToken cancellationToken = default)
  {
    string explainStatement = StatementTemplates.ExplainBy(queryId);

    return ExecuteStatementAsync<ExplainResponse>(explainStatement, cancellationToken);
  }

  /// <summary>
  /// Asserts that a topic exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  public Task<AssertTopicResponse[]> AssertTopicExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default)
  {
    return AssertTopicExistsAsync(options, exists: true, cancellationToken);
  }

  /// <summary>
  /// Asserts that a topic exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  public Task<AssertTopicResponse[]> AssertTopicNotExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default)
  {
    return AssertTopicExistsAsync(options, exists: false, cancellationToken);
  }

  private async Task<AssertTopicResponse[]> AssertTopicExistsAsync(AssertTopicOptions options, bool exists, CancellationToken cancellationToken = default)
  {
    string assertStatement = AssertTopic.CreateStatement(exists, options);

    KSqlDbStatement ksqlDbStatement = new(assertStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync<AssertTopicResponse>().ConfigureAwait(false);

    return statementResponse;
  }

  /// <summary>
  /// Asserts that a schema exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert schema options such as subject name, id and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert schema responses. If the assertion fails, then an error will be returned.</returns>
  public Task<AssertSchemaResponse[]> AssertSchemaExistsAsync(AssertSchemaOptions options, CancellationToken cancellationToken = default)
  {
    return AssertSchemaExistsAsync(options, exists: true, cancellationToken);
  }

  /// <summary>
  /// Asserts that a schema exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert schema options such as subject name, id and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert schema responses. If the assertion fails, then an error will be returned.</returns>
  public Task<AssertSchemaResponse[]> AssertSchemaNotExistsAsync(AssertSchemaOptions options, CancellationToken cancellationToken = default)
  {
    return AssertSchemaExistsAsync(options, exists: false, cancellationToken);
  }

  private async Task<AssertSchemaResponse[]> AssertSchemaExistsAsync(AssertSchemaOptions options, bool exists, CancellationToken cancellationToken = default)
  {
    string assertStatement = AssertSchema.CreateStatement(exists, options);

    KSqlDbStatement ksqlDbStatement = new(assertStatement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync<AssertSchemaResponse>().ConfigureAwait(false);

    return statementResponse;
  }

  private async Task<TResponse[]> ExecuteStatementAsync<TResponse>(string statement, CancellationToken cancellationToken = default)
  {
    KSqlDbStatement ksqlDbStatement = new(statement);

    var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync<TResponse>().ConfigureAwait(false);

    return statementResponse;
  }
}
