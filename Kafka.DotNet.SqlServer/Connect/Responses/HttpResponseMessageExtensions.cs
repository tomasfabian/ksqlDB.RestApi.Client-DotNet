using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kafka.DotNet.SqlServer.Connect.Responses
{
  public static class HttpResponseMessageExtensions
  {
    public static Task<CreateConnectorResponse> ToCreateConnectorResponse(this HttpResponseMessage httpResponseMessage)
    {
      return httpResponseMessage.To<CreateConnectorResponse>();
    }

    private static async Task<TEntity> To<TEntity>(this HttpResponseMessage httpResponseMessage)
    {
      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

      var responseObject = JsonSerializer.Deserialize<TEntity>(responseContent);

      return responseObject;
    }
  }
}