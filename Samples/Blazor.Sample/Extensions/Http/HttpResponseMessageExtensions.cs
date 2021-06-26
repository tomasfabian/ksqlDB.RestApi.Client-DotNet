using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Blazor.Sample.Extensions.Http
{
  public static class HttpResponseMessageExtensions
  {
    public static StatementResponse[] ToStatementResponses(this HttpResponseMessage httpResponseMessage)
    {
      StatementResponse[] statementResponses;

      if (httpResponseMessage.IsSuccessStatusCode)
      {
        statementResponses = httpResponseMessage.ToStatementResponses();
      }
      else
      {
        statementResponses = new[] { httpResponseMessage.ToStatementResponse()};
      }

      return statementResponses;
    }
  }
}