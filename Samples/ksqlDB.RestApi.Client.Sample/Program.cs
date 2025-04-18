using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.Samples.Json;
using ksqlDB.RestApi.Client.Samples.Models;
using ksqlDB.RestApi.Client.Samples.Models.InvocationFunctions;
using ksqlDB.RestApi.Client.Samples.Models.Movies;
using ksqlDB.RestApi.Client.Samples.Observers;
using ksqlDB.RestApi.Client.Samples.Providers;
using ksqlDB.RestApi.Client.Samples.PullQuery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using K = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql;
using HttpClientFactory = ksqlDB.RestApi.Client.Samples.Http.HttpClientFactory;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using EndpointType = ksqlDB.RestApi.Client.KSql.Query.Options.EndpointType;

namespace ksqlDB.RestApi.Client.Samples;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedVariable
public static class Program
{
  public static KSqlDBContextOptions CreateKSqlDbContextOptions(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDbContextOptionsBuilder()
      .UseKSqlDb(ksqlDbUrl)
      .SetBasicAuthCredentials("fred", "letmein")
      .SetJsonSerializerOptions(jsonOptions =>
      {
        jsonOptions.IgnoreReadOnlyFields = true;
        jsonOptions.TypeInfoResolver = SourceGenerationContext.Default;
      })
      //.SetAutoOffsetReset(AutoOffsetReset.Earliest) // global setting
      .SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce) // global setting
      .SetIdentifierEscaping(IdentifierEscaping.Keywords)
      .SetEndpointType(EndpointType.QueryStream) // uses HTTP/2.0
      //.SetEndpointType(EndpointType.Query) // uses HTTP/1.0
      .SetupPushQuery(options =>
      {
        options.Properties[KSqlDbConfigs.KsqlQueryPushV2Enabled] = "true";
      })
      .SetupPullQuery(options =>
      {
        options[KSqlDbConfigs.KsqlQueryPullTableScanEnabled] = "false";
      })
      .Options;

    return contextOptions;
  }

  public static async Task Main(string[] args)
  {
    var ksqlDbUrl = "http://localhost:8088";

    var loggerFactory = CreateLoggerFactory();

    var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

    var restApiClientOptions = new KSqlDBRestApiClientOptions
    {
      ShouldPluralizeFromItemName = true,
    };

    var restApiProvider = new KSqlDbRestApiProvider(httpClientFactory, restApiClientOptions, loggerFactory)
    {
      DisposeHttpClient = false
    };

    var moviesProvider = new MoviesProvider(restApiProvider);

    await moviesProvider.CreateTablesAsync();

    var cancellationTokenSource = new CancellationTokenSource();
    var contextOptions = CreateKSqlDbContextOptions(ksqlDbUrl);

    await using var context = new KSqlDBContext(contextOptions, loggerFactory);

    var query = context.CreatePushQuery<Movie>()
      .Where(p => p.Title != "E.T.")
      .Where(c => c.Title.ToLower().Contains("hard".ToLower()) || c.Id == 1)
      .Where(p => p.RowTime >= 1510923225000)
      .Take(2); // LIMIT 2    

    var ksql = query.ToQueryString();

    Console.WriteLine("Generated ksql:");
    Console.WriteLine(ksql);
    Console.WriteLine();

    using var disposable = query
      .ToObservable() // client side processing starts here lazily after subscription. Switches to Rx.NET
      .Finally(() => { Console.WriteLine("Finally"); })
      .Subscribe(onNext: movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
        Console.WriteLine();
      }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));

    await CreateOrReplaceTableStatement(context, cancellationTokenSource.Token);

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);
    await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor1);

    try
    {
      await new PullQueryExample().ExecuteAsync();
    }
    catch (Exception e)
    {
      Console.WriteLine();
      Console.WriteLine(e.Message);
    }

    string explain = await query.ExplainAsStringAsync(cancellationTokenSource.Token);
    Console.WriteLine("Explain: ");
    Console.WriteLine(explain);
    ExplainResponse[] explainResponses = await query.ExplainAsync(cancellationTokenSource.Token);
    Console.WriteLine($"{Environment.NewLine} Explain => ExecutionPlan:");
    Console.WriteLine(explainResponses[0].QueryDescription?.ExecutionPlan);

    Console.WriteLine("Press any key to stop the subscription");

    Console.ReadKey();

    await moviesProvider.DropTablesAsync();

    Console.WriteLine("Finished.");
  }

  static async Task CreateOrReplaceTableStatement(IKSqlDBStatementsContext context, CancellationToken cancellationToken = default)
  {
    var creationMetadata = new CreationMetadata
    {
      KafkaTopic = "moviesByTitle",
      KeyFormat = SerializationFormats.Json,
      ValueFormat = SerializationFormats.Json,
      Replicas = 1,
      Partitions = 1
    };

    var httpResponseMessage = await context.CreateOrReplaceTableStatement(tableName: "MoviesByTitle")
      .With(creationMetadata)
      .As<Movie>()
      .Where(c => c.Id < 3)
      .Select(c => new { c.Title, ReleaseYear = c.Release_Year })
      .PartitionBy(c => c.Title)
      .ExecuteStatementAsync(cancellationToken);

    /*
  CREATE OR REPLACE TABLE MoviesByTitle
  WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS = '1', REPLICAS='1' )
  AS SELECT Title, Release_Year AS ReleaseYear FROM Movies
  WHERE Id < 3 PARTITION BY Title EMIT CHANGES;
     */

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

    var statementResponse = await httpResponseMessage.ToStatementResponsesAsync();
  }

  internal class DebugHandler : DelegatingHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      System.Diagnostics.Debug.WriteLine($"Process request: {request.RequestUri}");

      return base.SendAsync(request, cancellationToken);
    }
  }

  private static async Task ConfigureKSqlDbWithServicesCollection_AndTryAsync(string ksqlDbUrl, CancellationToken cancellationToken = default)
  {
    var services = new ServiceCollection();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
    {
      c.UseKSqlDb(ksqlDbUrl);

      c.ReplaceHttpClient<KSql.RestApi.Http.IHttpClientFactory, KSql.RestApi.Http.HttpClientFactory>(_ => { })
        .AddHttpMessageHandler(_ => new DebugHandler());
    });

    var provider = services.BuildServiceProvider();

    var context = provider.GetRequiredService<IKSqlDBContext>();

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    using var d1 = context.CreatePushQuery<Movie>()
      .Take(2)
      //Movies are deserialized with SourceGenerationContext (see SetJsonSerializerOptions above)
      .Subscribe(onNext: movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
        Console.WriteLine();
      }, onError: error =>
      {
        semaphoreSlim.Release();

        Console.WriteLine($"Exception: {error.Message}");
      },
      onCompleted: () =>
      {
        semaphoreSlim.Release();
        Console.WriteLine("Completed");
      });

    await semaphoreSlim.WaitAsync(cancellationToken);

    await context.DisposeAsync();
  }

  private static async Task AddAndSaveChangesAsync(KSqlDBContext context, CancellationToken cancellationToken = default)
  {
    context.Add(MoviesProvider.Movie1);
    context.Add(MoviesProvider.Movie2);

    var saveResponse = await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task SubscribeAsync(IKSqlDBContext context)
  {
    var cts = new CancellationTokenSource();

    try
    {
      var subscription = await context.CreatePushQuery<Movie>()
        .SubscribeOn(ThreadPoolScheduler.Instance)
        .ObserveOn(TaskPoolScheduler.Default)
        .SubscribeAsync(onNext: movie =>
          {
            Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
            Console.WriteLine();
          }, onError: error => { Console.WriteLine($"SubscribeAsync Exception: {error.Message}"); },
          onCompleted: () => Console.WriteLine("SubscribeAsync Completed"), cts.Token);

      Console.WriteLine($"Query id: {subscription.QueryId}");
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    await Task.Delay(9000, cts.Token);

    await cts.CancelAsync();
  }

  private static IDisposable ClientSideBatching(KSqlDBContext context)
  {
    var disposable = context.CreatePushQuery<Tweet>()
      .ToObservable()
      .Buffer(TimeSpan.FromMilliseconds(250), 100)
      .Where(c => c.Count > 0)
      //.ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current) //WPF
      .Subscribe(tweets =>
      {
        foreach (var tweet in tweets)
        {
          Console.WriteLine(tweet.Message);
        }
      });

    return disposable;
  }

  private static async Task AsyncEnumerable(KSqlDBContext context)
  {
    var cts = new CancellationTokenSource();
    var asyncTweetsEnumerable = context.CreatePushQuery<Movie>().ToAsyncEnumerable();

    await foreach (var movie in asyncTweetsEnumerable.WithCancellation(cts.Token))
    {
      Console.WriteLine(movie.Title);

      await cts.CancelAsync();
    }
  }

  private static void Between(IKSqlDBContext context)
  {
    var ksql = context.CreatePushQuery<Tweet>().Where(c => c.Id.Between(1, 5))
      .ToQueryString();
  }

  private static IDisposable KQueryWithObserver(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    var context = new KSqlDBContext(contextOptions);

    var subscription = context.CreatePushQuery<Tweet>()
      .Where(p => p.Message != "Hello world" && p.Id != 1)
      .Take(2)
      .Subscribe(new TweetsObserver());

    return subscription;
  }

  private static IDisposable ToObservableExample(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    var context = new KSqlDBContext(contextOptions);

    var subscriptions = context.CreatePushQuery<Tweet>()
      .ToObservable()
      .Delay(TimeSpan.FromSeconds(2)) // IObservable extensions
      .Subscribe(new TweetsObserver());

    return subscriptions;
  }

  private static async Task DescribeFunction(IKSqlDbRestApiClient restApiProvider, string functionName, CancellationToken cancellationToken = default)
  {
    var httpResponseMessage = await restApiProvider.ExecuteStatementAsync(new KSqlDbStatement($"DESCRIBE FUNCTION {functionName};"), cancellationToken);
    var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
    Console.WriteLine(content);
  }

  private static async Task ToQueryStringExample(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    await using var context = new KSqlDBContext(contextOptions);

    var ksql = context.CreatePushQuery<Person>().ToQueryString();

    //prints SELECT * FROM People EMIT CHANGES;
    Console.WriteLine(ksql);
  }

  private static IDisposable NotNull(KSqlDBContext context)
  {
    return context.CreatePushQuery<Click>()
      .Where(c => c.IP_ADDRESS != null)
      .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
      .Subscribe(Console.WriteLine, error => { Console.WriteLine($"Exception: {error.Message}"); });
  }

  private static IDisposable DynamicFunctionCall(KSqlDBContext context)
  {
    var ifNullQueryString = context.CreatePushQuery<Tweet>()
      .Select(c => new { c.Id, c.Amount, Col = K.F.Dynamic("IFNULL(Message, 'n/a')") as string })
      .ToQueryString();

    return context.CreatePushQuery<Tweet>()
      .Select(c => K.Functions.Dynamic("ARRAY_DISTINCT(ARRAY[1, 1, 2, 3, 1, 2])") as int[])
      .Subscribe(
        array => Console.WriteLine($"{array![0]} - {array[^1]}"),
        error => Console.WriteLine($"Exception: {error.Message}"));
  }

  private static void ScalarFunctions(KSqlDBContext context)
  {
    context.CreatePushQuery<Tweet>()
      .Select(c => new
      {
        Abs = K.Functions.Abs(c.Amount),
        Ceil = K.Functions.Ceil(c.Amount),
        Floor = K.Functions.Floor(c.Amount),
        Random = K.Functions.Random(),
        Sign = K.Functions.Sign(c.Amount)
      })
      .ToQueryString();
  }

  private static IDisposable Entries(KSqlDBContext context)
  {
    bool sorted = true;

    var subscription = context.CreatePushQuery<Movie>()
      .Select(c => new
      {
        Entries = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
        {
            {"a", "value"},
            {"b", c.Title }
        }, sorted)
      })
      .Subscribe(c =>
      {
        Console.WriteLine("Entries:");

        foreach (var entry in c.Entries)
        {
          var key = entry.K;

          var value = entry.V;

          Console.WriteLine($"{key} - {value}");
        }
      }, _ => { });

    return subscription;
  }

  private static IDisposable QueryStreamRawKSql(KSqlDBContext context)
  {
    string ksql = context.CreatePushQuery<Movie>()
      .Where(p => p.Title != "E.T.").Take(2)
      .ToQueryString();

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      AutoOffsetReset = AutoOffsetReset.Earliest,
      [KSqlDbConfigs.KsqlQueryPushV2Enabled] = "true"
    };

    var disposable = context.CreatePushQuery<Movie>(queryStreamParameters)
      .ToObservable()
      .Subscribe(onNext: movie =>
        {
          Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); },
        onCompleted: () => Console.WriteLine("Completed"));

    return disposable;
  }

  private class Book
  {
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ReleaseYear { get; set; }
  }

  private static async Task InsertIntoSelectAsync(KSqlDbRestApiProvider restApiProvider, KSqlDBContext context, CancellationToken cancellationToken = default)
  {
    string streamName = "book";
    EntityCreationMetadata metadata = new(streamName)
    {
      EntityName = streamName,
      Partitions = 1,
      Replicas = 1,
      ValueFormat = SerializationFormats.Json,
      ShouldPluralizeEntityName = false
    };
    await restApiProvider.CreateOrReplaceStreamAsync<Book>(metadata, cancellationToken);
   
    string streamNameFrom = "book_from";
    metadata.EntityName = streamNameFrom;
    metadata.KafkaTopic = streamNameFrom;
    await restApiProvider.CreateOrReplaceStreamAsync<Book>(metadata, cancellationToken);

    string queryId = "insert_query_book";

    var response = await context.CreatePushQuery<Book>(streamNameFrom)
      .Where(c => c.Title != "Apocalypse now")
      .InsertIntoAsync(streamName, queryId, cancellationToken);

    var responses = await response.ToStatementResponsesAsync();
    Console.WriteLine($"QueryId: {responses[0].CommandStatus?.QueryId}");
  }

  private static IDisposable QueryRawKSql(IKSqlDBContext context)
  {
    string ksql = @"SELECT * FROM Movies
WHERE Title != 'E.T.' EMIT CHANGES LIMIT 2;";

    var queryParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToKSqlValue(),
    };

    var disposable = context.CreatePushQuery<Movie>(queryParameters)
      .ToObservable()
      .Subscribe(onNext: movie =>
        {
          Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); },
        onCompleted: () => Console.WriteLine("Completed"));

    return disposable;
  }

  private static void Cast(IKSqlDBContext context)
  {
    //SELECT Id, CAST(COUNT(*) AS VARCHAR) Count, CAST('42' AS INT) TheAnswer FROM Movies GROUP BY Id EMIT CHANGES;
    var query = context.CreatePushQuery<Movie>()
      .GroupBy(c => c.Id)
      .Select(c => new { Id = c.Key, Count = c.Count().ToString(), TheAnswer = KSQLConvert.ToInt32("42") })
      .ToQueryString();
  }

  private static void WithOffsetResetPolicy(IKSqlDBContext context)
  {
    var subscription = context.CreatePushQuery<Movie>()
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Subscribe(movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
      }, e => { Console.WriteLine($"Exception: {e.Message}"); });
  }

  private static void InvocationFunctions(IKSqlDBContext ksqlDbContext)
  {
    var ksql = ksqlDbContext.CreatePushQuery<Lambda>()
      .Select(c => new
      {
        Transformed = c.Lambda_Arr.Transform(x => x + 1),
        Filtered = c.Lambda_Arr.Filter(x => x > 1),
        Acc = c.Lambda_Arr.Reduce(0, (x, y) => x + y)
      })
      .ToQueryString();

    Console.WriteLine(ksql);

    var ksqlMap = ksqlDbContext.CreatePushQuery<Lambda>()
      .Select(c => new
      {
        Transformed = K.Functions.Transform(c.DictionaryArrayValues, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v, x => x * x)),
        Filtered = K.Functions.Filter(c.DictionaryInValues, (k, v) => k != "E.T" && v > 0),
        Acc = K.Functions.Reduce(c.DictionaryInValues, 2, (s, k, v) => K.Functions.Ceil(s / v))
      })
      .ToQueryString();
  }

  public static ILoggerFactory CreateLoggerFactory()
  {
    var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
    var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
    var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());

    var consoleLoggerProvider = new ConsoleLoggerProvider(optionsMonitor);

    var loggerFactory = new LoggerFactory();
    loggerFactory.AddProvider(consoleLoggerProvider);

    return loggerFactory;
  }
}
