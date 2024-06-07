using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Extensions;

public static class HttpResponseMessageExtensions
{
  public static StatementResponse[] ToStatementResponses(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    string responseContent = httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).Result;
      
    var responseObjects = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

    return responseObjects ?? [];
  }

  public static StatementResponse? ToStatementResponse(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    string responseContent = httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).Result;
      
    var responseObject = JsonSerializer.Deserialize<StatementResponse>(responseContent);

    return responseObject;
  }

  public static Task<StatementResponse[]> ToStatementResponsesAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<StatementResponse>(cancellationToken);
  }

  public static Task<StreamsResponse[]> ToStreamsResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<StreamsResponse>(cancellationToken);
  }

  public static Task<TablesResponse[]> ToTablesResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<TablesResponse>(cancellationToken);
  }

  public static Task<QueriesResponse[]> ToQueriesResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<QueriesResponse>(cancellationToken);
  }

  public static Task<TopicsResponse[]> ToTopicsResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<TopicsResponse>(cancellationToken);
  }
	
  public static Task<TopicsExtendedResponse[]> ToTopicsExtendedResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<TopicsExtendedResponse>(cancellationToken);
  }

  public static Task<ConnectorsResponse[]> ToConnectorsResponseAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    return httpResponseMessage.ToStatementResponsesAsync<ConnectorsResponse>(cancellationToken);
  }

  internal static async Task<TResponse[]> ToStatementResponsesAsync<TResponse>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    TResponse[]? statementResponses;

    if (httpResponseMessage.IsSuccessStatusCode)
      statementResponses = await httpResponseMessage.ToStatementResponseAsync<TResponse[]>(cancellationToken).ConfigureAwait(false);
    else
    {
      var statementResponse = await httpResponseMessage.ToStatementResponseAsync<TResponse>(cancellationToken).ConfigureAwait(false);
      statementResponses = statementResponse != null ? [statementResponse] : [];
    }

    return statementResponses ?? [];
  }

  private static readonly JsonSerializerOptions JsonSerializerOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };
    
  private static async Task<TResponse?> ToStatementResponseAsync<TResponse>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
      
    var responseObject = JsonSerializer.Deserialize<TResponse>(responseContent, JsonSerializerOptions);

    return responseObject;
  }
}
