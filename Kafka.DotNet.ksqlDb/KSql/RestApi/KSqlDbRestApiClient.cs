using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Streams;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Tables;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Connectors;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public class KSqlDbRestApiClient : IKSqlDbRestApiClient
  {
    private readonly IHttpClientFactory httpClientFactory;

    public KSqlDbRestApiClient(IHttpClientFactory httpClientFactory)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    internal static readonly string MediaType = "application/vnd.ksql.v1+json";

    private string NullOrWhiteSpaceErrorMessage => "Can't be null, empty, or contain only whitespace.";

    /// <summary>
    /// Run a sequence of SQL statements.
    /// </summary>
    /// <param name="ksqlDbStatement">The text of the SQL statements.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
    {
      return ExecuteStatementAsync(ksqlDbStatement, ksqlDbStatement.EndpointType, ksqlDbStatement.ContentEncoding, cancellationToken);
    }

    private async Task<HttpResponseMessage> ExecuteStatementAsync(object content, EndpointType endPointType, Encoding encoding, CancellationToken cancellationToken = default)
    {
      using var httpClient = httpClientFactory.CreateClient();

      var httpRequestMessage = CreateHttpRequestMessage(content, endPointType, encoding);

      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue(MediaType));

      var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken)
        .ConfigureAwait(false);

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

      return httpRequestMessage;
    }

    internal StringContent CreateContent(KSqlDbStatement ksqlDbStatement)
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
        _ => throw new ArgumentOutOfRangeException()
      };

      return endpoint;
    }

    #region Creation

    /// <summary>
    /// Create a new stream with the specified columns and properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
    /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a stream with the same name already exists.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns>Http response object.</returns>
    public Task<HttpResponseMessage> CreateStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
    {
      var ksql = StatementGenerator.CreateStream<T>(creationMetadata, ifNotExists);

      return ExecuteAsync<T>(ksql, cancellationToken);
    }

    /// <summary>
    /// Create a new stream or replace an existing one with the specified columns and properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns>Http response object.</returns>
    public Task<HttpResponseMessage> CreateOrReplaceStreamAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default)
    {
      var ksql = StatementGenerator.CreateOrReplaceStream<T>(creationMetadata);

      return ExecuteAsync<T>(ksql, cancellationToken);
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
      var ksql = StatementGenerator.CreateTable<T>(creationMetadata, ifNotExists);

      return ExecuteAsync<T>(ksql, cancellationToken);
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
      var ksql = StatementGenerator.CreateOrReplaceTable<T>(creationMetadata);

      return ExecuteAsync<T>(ksql, cancellationToken);
    }

    private Task<HttpResponseMessage> ExecuteAsync<T>(string ksql, CancellationToken cancellationToken = default)
    {
      var ksqlStatement = new KSqlDbStatement(ksql);

      return ExecuteStatementAsync(ksqlStatement, cancellationToken);
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
    public Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties insertProperties = null, CancellationToken cancellationToken = default)
    {
      var insert = new CreateInsert().Generate<T>(entity, insertProperties);

      KSqlDbStatement ksqlDbStatement = new(insert);

      var httpResponseMessage = ExecuteStatementAsync(ksqlDbStatement, cancellationToken);

      return httpResponseMessage;
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

      var streamsResponses = await httpResponseMessage.ToTablesResponseAsync().ConfigureAwait(false);

      return streamsResponses;
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
      if (config == null) throw new ArgumentNullException(nameof(config));

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
      if (config == null) throw new ArgumentNullException(nameof(config));

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
      string dropIfExistsStatement = $"DROP CONNECTOR IF EXISTS {connectorName};";

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

    internal Task<HttpResponseMessage> DropStreamAsync(string streamName, CancellationToken cancellationToken = default)
    {
      string dropStatement = StatementTemplates.DropStream(streamName);

      KSqlDbStatement ksqlDbStatement = new(dropStatement);

      return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }

    internal Task<HttpResponseMessage> DropTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
      string dropStatement = StatementTemplates.DropTable(tableName);

      KSqlDbStatement ksqlDbStatement = new(dropStatement);

      return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }

    /// <summary>
    /// Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.
    /// </summary>
    /// <param name="queryId">Id of the query to terminate.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns></returns>
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
    /// <param name="queryId">Id of the query to terminate.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns></returns>
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

    private async Task<TResponse[]> ExecuteStatementAsync<TResponse>(string statement, CancellationToken cancellationToken = default)
    {
      KSqlDbStatement ksqlDbStatement = new(statement);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

      var statementResponse = await httpResponseMessage.ToStatementResponsesAsync<TResponse>().ConfigureAwait(false);

      return statementResponse;
    }
  }
}