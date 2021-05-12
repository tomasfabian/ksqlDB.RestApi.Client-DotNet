using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
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

    public async Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
    {
      using var httpClient = httpClientFactory.CreateClient();

      var httpRequestMessage = CreateHttpRequestMessage(ksqlDbStatement);

      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue(MediaType));

      var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken)
        .ConfigureAwait(false);

      return httpResponseMessage;
    }

    internal HttpRequestMessage CreateHttpRequestMessage(KSqlDbStatement ksqlDbStatement)
    {
      var data = CreateContent(ksqlDbStatement);

      var endpoint = GetEndpoint(ksqlDbStatement);

      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
      {
        Content = data
      };

      return httpRequestMessage;
    }

    internal StringContent CreateContent(KSqlDbStatement ksqlDbStatement)
    {
      var json = JsonSerializer.Serialize(ksqlDbStatement);

      var data = new StringContent(json, ksqlDbStatement.ContentEncoding, MediaType);

      return data;
    }

    internal static string GetEndpoint(KSqlDbStatement ksqlDbStatement)
    {
      var endpoint = ksqlDbStatement.EndpointType switch
      {
        EndpointType.KSql => "/ksql",
        EndpointType.Query => "/query",
        _ => throw new ArgumentOutOfRangeException()
      };

      return endpoint;
    }

    #region Creation
    
    public Task<HttpResponseMessage> CreateStream<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
    {
      var ksql = StatementGenerator.CreateStream<T>(creationMetadata, ifNotExists);

      return ExecuteAsync<T>(ksql, cancellationToken);
    }

    public Task<HttpResponseMessage> CreateOrReplaceStream<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default)
    {
      var ksql = StatementGenerator.CreateOrReplaceStream<T>(creationMetadata);

      return ExecuteAsync<T>(ksql, cancellationToken);
    }
    
    public Task<HttpResponseMessage> CreateTable<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default)
    {
      var ksql = StatementGenerator.CreateTable<T>(creationMetadata, ifNotExists);

      return ExecuteAsync<T>(ksql, cancellationToken);
    }

    public Task<HttpResponseMessage> CreateOrReplaceTable<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default)
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

    public Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties insertProperties = null, CancellationToken cancellationToken = default)
    {
      var insert = new CreateInsert().Generate<T>(entity, insertProperties);
	
      KSqlDbStatement ksqlDbStatement = new(insert);

      var httpResponseMessage = ExecuteStatementAsync(ksqlDbStatement, cancellationToken);

      return httpResponseMessage;
    }
  }
}