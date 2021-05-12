using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{    
  internal abstract class KSqlDbProvider : IKSqlDbProvider
  {
    private readonly IHttpClientFactory httpClientFactory;

    protected KSqlDbProvider(IHttpClientFactory httpClientFactory)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public abstract string ContentType { get; }

    protected abstract string QueryEndPointName { get; }

    protected virtual HttpClient OnCreateHttpClient()
    {
      return httpClientFactory.CreateClient();
    }

    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
    public async IAsyncEnumerable<T> Run<T>(object parameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      using var httpClient = OnCreateHttpClient();

      var httpRequestMessage = CreateQueryHttpRequestMessage(httpClient, parameters);

      //https://docs.ksqldb.io/en/latest/developer-guide/api/
      var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken)
        .ConfigureAwait(false);

#if NET
      var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
      var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
      
      using var streamReader = new StreamReader(stream);

      while (!streamReader.EndOfStream)
      {
        if (cancellationToken.IsCancellationRequested)
          yield break;

        var rawJson = await streamReader.ReadLineAsync()
          .ConfigureAwait(false);

        var record = OnLineRead<T>(rawJson);

        if (record != null) yield return record.Value;
      }
    }

    protected abstract RowValue<T> OnLineRead<T>(string rawJson);

    private JsonSerializerOptions jsonSerializerOptions;

    protected JsonSerializerOptions GetOrCreateJsonSerializerOptions()
    {
      if (jsonSerializerOptions == null)
        jsonSerializerOptions = OnCreateJsonSerializerOptions();

      return jsonSerializerOptions;
    }

    protected virtual JsonSerializerOptions OnCreateJsonSerializerOptions()
    {
      jsonSerializerOptions = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      return jsonSerializerOptions;
    }

    protected virtual HttpRequestMessage CreateQueryHttpRequestMessage(HttpClient httpClient, object parameters)
    {
      var json = JsonSerializer.Serialize(parameters);

      var data = new StringContent(json, Encoding.UTF8, "application/json");

      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue(ContentType));

      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, QueryEndPointName)
      {
        Content = data
      };

      return httpRequestMessage;
    }
  }
}