using System.Net.Http;
using System.Text.Json;
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
  }
}