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

namespace ksqlDB.RestApi.Client.Samples;

public static class Program
{
  public static KSqlDBContextOptions CreateQueryStreamOptions(string ksqlDbUrl)
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
      .SetupQueryStream(options =>
      {
        //SetupQueryStream affects only IKSqlDBContext.CreateQueryStream<T>
        options.AutoOffsetReset = AutoOffsetReset.Earliest;
      })
      .SetupQuery(options =>
      {
        //SetupQuery affects only IKSqlDBContext.CreateQuery<T>
        options.Properties[KSqlDbConfigs.ProcessingGuarantee] = ProcessingGuarantee.ExactlyOnce.ToKSqlValue();
      })
      .Options;

    contextOptions.DisposeHttpClient = false;

    return contextOptions;
  }

  public static async Task Main(string[] args)
  {
    var ksqlDbUrl = @"http://localhost:8088";

    var loggerFactory = CreateLoggerFactory();

    var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

    var restApiProvider = new KSqlDbRestApiProvider(httpClientFactory, loggerFactory)
    {
      DisposeHttpClient = false
    };

    var moviesProvider = new MoviesProvider(restApiProvider);

    await moviesProvider.CreateTablesAsync();

    var contextOptions = CreateQueryStreamOptions(ksqlDbUrl);

    await using var context = new KSqlDBContext(contextOptions, loggerFactory);

    var query = context.CreateQueryStream<Movie>() // Http 2.0
      // var query = context.CreateQuery<Movie>() // Http 1.0
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

    await CreateOrReplaceTableStatement(context);

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

    string explain = await query.ExplainAsStringAsync();
    ExplainResponse[] explainResponses = await query.ExplainAsync();
    Console.WriteLine($"{Environment.NewLine} Explain => ExecutionPlan:");
    Console.WriteLine(explainResponses[0].QueryDescription?.ExecutionPlan);

    Console.WriteLine("Press any key to stop the subscription");

    Console.ReadKey();

    await moviesProvider.DropTablesAsync();

    Console.WriteLine("Finished.");
  }

  static async Task CreateOrReplaceTableStatement(IKSqlDBStatementsContext context)
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
      .ExecuteStatementAsync();

    /*
  CREATE OR REPLACE TABLE MoviesByTitle
  WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS = '1', REPLICAS='1' )
  AS SELECT Title, Release_Year AS ReleaseYear FROM Movies
  WHERE Id < 3 PARTITION BY Title EMIT CHANGES;
     */

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

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

  private static async Task ConfigureKSqlDbWithServicesCollection_AndTryAsync(string ksqlDbUrl)
  {
    var services = new ServiceCollection();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
    {
      c.UseKSqlDb(ksqlDbUrl);

      c.ReplaceHttpClient<ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory, ksqlDB.RestApi.Client.KSql.RestApi.Http.HttpClientFactory>(_ => { })
        .AddHttpMessageHandler(_ => new DebugHandler());
    });

    var provider = services.BuildServiceProvider();

    var context = provider.GetRequiredService<IKSqlDBContext>();

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    using var d1 = context.CreateQueryStream<Movie>()
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

    await semaphoreSlim.WaitAsync();

    await context.DisposeAsync();
  }

  private static async Task AddAndSaveChangesAsync(KSqlDBContext context)
  {
    context.Add(MoviesProvider.Movie1);
    context.Add(MoviesProvider.Movie2);

    var saveResponse = await context.SaveChangesAsync();
  }

  private static async Task SubscribeAsync(IKSqlDBContext context, IKSqlDbRestApiClient restApiProvider)
  {
    var cts = new CancellationTokenSource();

    try
    {
      var subscription = await context.CreateQueryStream<Movie>()
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

    cts.Cancel();
  }

  private static IDisposable ClientSideBatching(KSqlDBContext context)
  {
    var disposable = context.CreateQueryStream<Tweet>()
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
    var asyncTweetsEnumerable = context.CreateQueryStream<Movie>().ToAsyncEnumerable();

    await foreach (var movie in asyncTweetsEnumerable.WithCancellation(cts.Token))
    {
      Console.WriteLine(movie.Title);

      await cts.CancelAsync();
    }
  }

  private static void Between(IKSqlDBContext context)
  {
    var ksql = context.CreateQueryStream<Tweet>().Where(c => c.Id.Between(1, 5))
      .ToQueryString();
  }

  private static IDisposable KQueryWithObserver(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    var context = new KSqlDBContext(contextOptions);

    var subscription = context.CreateQueryStream<Tweet>()
      .Where(p => p.Message != "Hello world" && p.Id != 1)
      .Take(2)
      .Subscribe(new TweetsObserver());

    return subscription;
  }

  private static IDisposable ToObservableExample(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    var context = new KSqlDBContext(contextOptions);

    var subscriptions = context.CreateQueryStream<Tweet>()
      .ToObservable()
      .Delay(TimeSpan.FromSeconds(2)) // IObservable extensions
      .Subscribe(new TweetsObserver());

    return subscriptions;
  }

  private static async Task DescribeFunction(IKSqlDbRestApiClient restApiProvider, string functionName)
  {
    var httpResponseMessage = await restApiProvider.ExecuteStatementAsync(new KSqlDbStatement($"DESCRIBE FUNCTION {functionName};"));
    var content = await httpResponseMessage.Content.ReadAsStringAsync();
    Console.WriteLine(content);
  }

  private static async Task ToQueryStringExample(string ksqlDbUrl)
  {
    var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
    await using var context = new KSqlDBContext(contextOptions);

    var ksql = context.CreateQueryStream<Person>().ToQueryString();

    //prints SELECT * FROM People EMIT CHANGES;
    Console.WriteLine(ksql);
  }

  private static IDisposable NotNull(KSqlDBContext context)
  {
    return context.CreateQueryStream<Click>()
      .Where(c => c.IP_ADDRESS != null)
      .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
      .Subscribe(Console.WriteLine, error => { Console.WriteLine($"Exception: {error.Message}"); });
  }

  private static IDisposable DynamicFunctionCall(KSqlDBContext context)
  {
    var ifNullQueryString = context.CreateQueryStream<Tweet>()
      .Select(c => new { c.Id, c.Amount, Col = K.F.Dynamic("IFNULL(Message, 'n/a')") as string })
      .ToQueryString();

    return context.CreateQueryStream<Tweet>()
      .Select(c => K.Functions.Dynamic("ARRAY_DISTINCT(ARRAY[1, 1, 2, 3, 1, 2])") as int[])
      .Subscribe(
        array => Console.WriteLine($"{array![0]} - {array[^1]}"),
        error => Console.WriteLine($"Exception: {error.Message}"));
  }

  private static void ScalarFunctions(KSqlDBContext context)
  {
    context.CreateQueryStream<Tweet>()
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

    var subscription = context.CreateQueryStream<Movie>()
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
      }, error => { });

    return subscription;
  }

  private static IDisposable QueryStreamRawKSql(KSqlDBContext context)
  {
    string ksql = context.CreateQueryStream<Movie>()
      .Where(p => p.Title != "E.T.").Take(2)
      .ToQueryString();

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var disposable = context.CreateQueryStream<Movie>(queryStreamParameters)
      .ToObservable()
      .Subscribe(onNext: movie =>
        {
          Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); },
        onCompleted: () => Console.WriteLine("Completed"));

    return disposable;
  }

  private static IDisposable QueryRawKSql(IKSqlDBContext context)
  {
    string ksql = @"SELECT * FROM Movies
WHERE Title != 'E.T.' EMIT CHANGES LIMIT 2;";

    QueryParameters queryParameters = new QueryParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var disposable = context.CreateQuery<Movie>(queryParameters)
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
    var query = context.CreateQueryStream<Movie>()
      .GroupBy(c => c.Id)
      .Select(c => new { Id = c.Key, Count = c.Count().ToString(), TheAnswer = KSQLConvert.ToInt32("42") })
      .ToQueryString();
  }

  private static void WithOffsetResetPolicy(IKSqlDBContext context)
  {
    var subscription = context.CreateQueryStream<Movie>()
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Subscribe(movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
      }, e => { Console.WriteLine($"Exception: {e.Message}"); });
  }

  private static void InvocationFunctions(IKSqlDBContext ksqlDbContext)
  {
    var ksql = ksqlDbContext.CreateQueryStream<Lambda>()
      .Select(c => new
      {
        Transformed = c.Lambda_Arr.Transform(x => x + 1),
        Filtered = c.Lambda_Arr.Filter(x => x > 1),
        Acc = c.Lambda_Arr.Reduce(0, (x, y) => x + y)
      })
      .ToQueryString();

    Console.WriteLine(ksql);

    var ksqlMap = ksqlDbContext.CreateQueryStream<Lambda>()
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
