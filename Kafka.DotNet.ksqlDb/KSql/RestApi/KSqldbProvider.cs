using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query;

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

    public async Task<Query<T>> RunAsync<T>(object parameters, CancellationToken cancellationToken = default)
    {
      var streamReader = await GetStreamReaderAsync<T>(parameters, cancellationToken).ConfigureAwait(false);

      cancellationToken.Register(() => streamReader?.Dispose());

      var queryId = await ReadHeaderAsync<T>(streamReader).ConfigureAwait(false);

      return new Query<T>
      {
        EnumerableQuery = ConsumeAsync<T>(streamReader, cancellationToken),
        QueryId = queryId
      };
    }

    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
    public async IAsyncEnumerable<T> Run<T>(object parameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      using var streamReader = await GetStreamReaderAsync<T>(parameters, cancellationToken).ConfigureAwait(false);

      await foreach (var entity in ConsumeAsync<T>(streamReader, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        yield return entity;
    }

    private async Task<StreamReader> GetStreamReaderAsync<T>(object parameters, CancellationToken cancellationToken)
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

      var streamReader = new StreamReader(stream);

      return streamReader;
    }

    private async IAsyncEnumerable<T> ConsumeAsync<T>(StreamReader streamReader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
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

    private async Task<string> ReadHeaderAsync<T>(StreamReader streamReader)
    {
      var rawJson = await streamReader.ReadLineAsync()
        .ConfigureAwait(false);

      return OnReadHeader<T>(rawJson);
    }

    protected abstract string OnReadHeader<T>(string rawJson);

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