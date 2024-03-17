using System.Net;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

internal static class HttpResponseMessageExtensions
{
  public static bool IsSuccess(this HttpResponseMessage httpResponseMessage)
  {
    try
    {
      if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
      {
        var responsesObject = httpResponseMessage.ToStatementResponses();

        var isSuccess = responsesObject.All(c => c.CommandStatus!.Status == CommandStatus.Success);

        return isSuccess;
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
        
      return false;
    }

    return false;
  }
}
