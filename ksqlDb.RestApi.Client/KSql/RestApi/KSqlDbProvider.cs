using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

internal abstract class KSqlDbProvider : IKSqlDbProvider
{
  private readonly IHttpClientFactory httpClientFactory;
  private readonly KSqlDbProviderOptions options;
  private readonly ILogger logger;

  protected KSqlDbProvider(IHttpClientFactory httpClientFactory, KSqlDbProviderOptions options, ILogger logger = null)
  {
    this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    this.options = options ?? throw new ArgumentNullException(nameof(options));
    this.logger = logger;
  }

  public abstract string ContentType { get; }

  protected abstract string QueryEndPointName { get; }

  internal KSqlDbProviderOptions Options => options;

  protected virtual HttpClient OnCreateHttpClient()
  {
    return httpClientFactory.CreateClient();
  }

  public async Task<QueryStream<T>> RunAsync<T>(object parameters, CancellationToken cancellationToken = default)
  {
    logger?.LogInformation($"Executing query {parameters}");

    var streamReader = await TryGetStreamReaderAsync<T>(parameters, cancellationToken).ConfigureAwait(false);

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    cancellationToken.Register(() => semaphoreSlim.Release());

    var queryId = await ReadHeaderAsync<T>(streamReader).ConfigureAwait(false);

    return new QueryStream<T>
    {
      EnumerableQuery = ConsumeAsync<T>(streamReader, semaphoreSlim, cancellationToken),
      QueryId = queryId
    };
  }

  /// <param name="parameters">Query parameters</param>
  /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
  public async IAsyncEnumerable<T> Run<T>(object parameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    logger?.LogInformation($"Executing query {parameters}");

    using var streamReader = await TryGetStreamReaderAsync<T>(parameters, cancellationToken).ConfigureAwait(false);

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    cancellationToken.Register(() =>
    {
      semaphoreSlim.Release();
    });

    await foreach (var entity in ConsumeAsync<T>(streamReader, semaphoreSlim, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
      yield return entity;
  }

  private async Task<StreamReader> TryGetStreamReaderAsync<T>(object parameters, CancellationToken cancellationToken)
  {
    try
    {
      return await GetStreamReaderAsync<T>(parameters, cancellationToken);
    }
    catch (Exception exception)
    {
      logger?.LogError(exception, "Query execution failed.");

      throw;
    }
  }

  private async Task<StreamReader> GetStreamReaderAsync<T>(object parameters, CancellationToken cancellationToken)
  {
    var httpClient = OnCreateHttpClient();

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

    if (options.DisposeHttpClient)
      httpClient.Dispose();

    return streamReader;
  }

  private static Task<bool> EndOfStreamAsync(StreamReader streamReader, CancellationToken cancellationToken)
  {
    return Task.Run(() =>
    {
      try
      {
        return !streamReader.EndOfStream;
      }
      catch (Exception)
      {
        if (!cancellationToken.IsCancellationRequested)
          throw;

        return true;
      }

    }, cancellationToken);
  }

  /// <summary>
  /// Asynchronously consumes data from a <see cref="StreamReader"/> and yields elements of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements to yield.</typeparam>
  /// <param name="streamReader">The <see cref="StreamReader"/> to read data from.</param>
  /// <param name="semaphoreSlim">The <see cref="SemaphoreSlim"/> used for synchronization.</param>
  /// <param name="cancellationToken">The cancellation token to observe.</param>
  /// <returns>An asynchronous enumerable of elements of type <typeparamref name="T"/>.</returns>
  private async IAsyncEnumerable<T> ConsumeAsync<T>(StreamReader streamReader, SemaphoreSlim semaphoreSlim, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
      yield break;

    var cancellationTask = Task.Run(async () =>
    {
      await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

      return false;
    }, cancellationToken);

    while (await await Task.WhenAny(EndOfStreamAsync(streamReader, cancellationToken), cancellationTask).ConfigureAwait(false))
    {
      if (cancellationToken.IsCancellationRequested)
        yield break;
        
      var rawData = await streamReader
#if NET7_0_OR_GREATER
        .ReadLineAsync(cancellationToken)
#else
        .ReadLineAsync()
#endif
        .ConfigureAwait(false);

      logger?.LogDebug($"Raw data received: {rawData}");

      var record = OnLineRead<T>(rawData);

      if (record != null) yield return record.Value;
    }

    if (!cancellationToken.IsCancellationRequested)
      semaphoreSlim.Release();
  }

  private async Task<string> ReadHeaderAsync<T>(StreamReader streamReader)
  {
    var rawData = await streamReader.ReadLineAsync()
      .ConfigureAwait(false);

    return OnReadHeader<T>(rawData);
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
    return options.JsonSerializerOptions;
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

  protected bool IsErrorRow(string rawJson)
  {
    return KSqlDbProviderValueReader.IsErrorRow(rawJson);
  }
}
