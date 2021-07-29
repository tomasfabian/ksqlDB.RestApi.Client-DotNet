using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Streams;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Tables;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions
{
  public static class HttpResponseMessageExtensions
  {
    public static StatementResponse[] ToStatementResponses(this HttpResponseMessage httpResponseMessage)
    {
      string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
      
      var responseObjects = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

      return responseObjects;
    }

    public static StatementResponse ToStatementResponse(this HttpResponseMessage httpResponseMessage)
    {
      string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
      
      var responseObject = JsonSerializer.Deserialize<StatementResponse>(responseContent);

      return responseObject;
    }

    public static Task<StatementResponse[]> ToStatementResponsesAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<StatementResponse>();
    }

    public static Task<StreamsResponse[]> ToStreamsResponseAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<StreamsResponse>();
    }

    public static Task<TablesResponse[]> ToTablesResponseAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<TablesResponse>();
    }

    public static Task<TopicsResponse[]> ToTopicsResponseAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<TopicsResponse>();
    }
	
    public static Task<TopicsExtendedResponse[]> ToTopicsExtendedResponseAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<TopicsExtendedResponse>();
    }

    public static Task<ConnectorsResponse[]> ToConnectorsResponseAsync(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.ToStatementResponsesAsync<ConnectorsResponse>();
    }

    private static async Task<TResponse[]> ToStatementResponsesAsync<TResponse>(this HttpResponseMessage httpResponseMessage)
    {
      TResponse[] statementResponses;

      if (httpResponseMessage.IsSuccessStatusCode)
        statementResponses = await httpResponseMessage.ToStatementResponseAsync<TResponse[]>().ConfigureAwait(false);
      else
        statementResponses = new[] { await httpResponseMessage.ToStatementResponseAsync<TResponse>().ConfigureAwait(false) };

      return statementResponses;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
      PropertyNameCaseInsensitive = true
    };
    
    private static async Task<TResponse> ToStatementResponseAsync<TResponse>(this HttpResponseMessage httpResponseMessage)
    {
      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
      
      var responseObject = JsonSerializer.Deserialize<TResponse>(responseContent, JsonSerializerOptions);

      return responseObject;
    }
  }
}