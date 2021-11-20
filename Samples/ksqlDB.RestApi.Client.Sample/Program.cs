using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.Api.Client.Samples.HostedServices;
using ksqlDB.Api.Client.Samples.Models;
using ksqlDB.Api.Client.Samples.Models.Events;
using ksqlDB.Api.Client.Samples.Models.InvocationFunctions;
using ksqlDB.Api.Client.Samples.Models.Movies;
using ksqlDB.Api.Client.Samples.Observers;
using ksqlDB.Api.Client.Samples.Providers;
using ksqlDB.Api.Client.Samples.PullQuery;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using K = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql;

namespace ksqlDB.Api.Client.Samples
{
  public static class Program
  {
    public static KSqlDBContextOptions CreateQueryStreamOptions(string ksqlDbUrl)
    {
      var contextOptions = new KSqlDbContextOptionsBuilder()
        .UseKSqlDb(ksqlDbUrl)
        .SetBasicAuthCredentials("fred", "letmein")
        //.SetAutoOffsetReset(AutoOffsetReset.Earliest) // global setting
        .SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce) // global setting
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
      
      return contextOptions;
    }

    public static async Task Main(string[] args)
    {
      var ksqlDbUrl = @"http:\\localhost:8088";
      
      var loggerFactory = CreateLoggerFactory();

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
      var restApiProvider = new KSqlDbRestApiProvider(httpClientFactory, loggerFactory);
      restApiProvider.SetCredentials(new BasicAuthCredentials("fred", "letmein"));
      
      var moviesProvider = new MoviesProvider(restApiProvider);

      await moviesProvider.CreateTablesAsync();

      var contextOptions = CreateQueryStreamOptions(ksqlDbUrl);

      await using var context = new KSqlDBContext(contextOptions, loggerFactory);

      var query = context.CreateQueryStream<Movie>() // Http 2.0
      // var query = context.CreateQuery<Movie>() // Http 1.0
        .Where(p => p.Title != "E.T.")
        .Where(c => c.Title.ToLower().Contains("hard".ToLower()) || c.Id == 1)
        .Where(p => p.RowTime >= 1510923225000)
        .Select(l => new { Id = l.Id, l.Title, l.Release_Year, l.RowTime })
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

    private static async Task AddAndSaveChangesAsync(KSqlDBContext context)
    {
      context.Add(MoviesProvider.Movie1);
      context.Add(MoviesProvider.Movie2);

      var saveResponse = await context.SaveChangesAsync();
    }

    private static async Task SubscribeAsync(IKSqlDBContext context)
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

    private static async Task GetKsqlDbInformationAsync(KSqlDbRestApiProvider restApiProvider)
    {
      Console.WriteLine($"{Environment.NewLine}Available topics:");
      var topicsResponses = await restApiProvider.GetTopicsAsync();
      Console.WriteLine(string.Join(',', topicsResponses[0].Topics.Select(c => c.Name)));

      TopicsResponse[] allTopicsResponses = await restApiProvider.GetAllTopicsAsync();
      TopicsExtendedResponse[] topicsExtendedResponses = await restApiProvider.GetTopicsExtendedAsync();
      var allTopicsExtendedResponses = await restApiProvider.GetAllTopicsExtendedAsync();

      Console.WriteLine($"{Environment.NewLine}Available tables:");
      var tablesResponse = await restApiProvider.GetTablesAsync();
      Console.WriteLine(string.Join(',', tablesResponse[0].Tables.Select(c => c.Name)));

      Console.WriteLine($"{Environment.NewLine}Available streams:");
      var streamsResponse = await restApiProvider.GetStreamsAsync();
      Console.WriteLine(string.Join(',', streamsResponse[0].Streams.Select(c => c.Name)));

      Console.WriteLine($"{Environment.NewLine}Available connectors:");
      var connectorsResponse = await restApiProvider.GetConnectorsAsync();
      Console.WriteLine(string.Join(',', connectorsResponse[0].Connectors.Select(c => c.Name)));
    }

    private static async Task CreateOrReplaceTableStatement(IKSqlDBStatementsContext context)
    {
      var creationMetadata = new CreationMetadata
      {
        KafkaTopic = "tweetsByTitle",
        KeyFormat = SerializationFormats.Json,
        ValueFormat = SerializationFormats.Json,
        Replicas = 1,
        Partitions = 1
      };

      var httpResponseMessage = await context.CreateOrReplaceTableStatement(tableName: "TweetsByTitle")
        .With(creationMetadata)
        .As<Movie>()
        .Where(c => c.Id < 3)
        .Select(c => new { c.Title, ReleaseYear = c.Release_Year })
        .PartitionBy(c => c.Title)
        .ExecuteStatementAsync();

      /*
CREATE OR REPLACE TABLE TweetsByTitle
 WITH ( KAFKA_TOPIC='tweetsByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS = '1', REPLICAS='1' )
AS SELECT Title, Release_Year AS ReleaseYear FROM Movies
WHERE Id < 3 PARTITION BY Title EMIT CHANGES;
       */

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

      var statementResponse = await httpResponseMessage.ToStatementResponsesAsync();
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

    private static IDisposable JoinTables(KSqlDBContext context)
    {
      var query = context.CreateQueryStream<Movie>()
        .Join(
          //.LeftJoin(
          Source.Of<Lead_Actor>(nameof(Lead_Actor)),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            movie.Id,
            Title = movie.Title,
            movie.Release_Year,
            ActorName = K.Functions.RPad(K.Functions.LPad(actor.Actor_Name.ToUpper(), 15, "*"), 25, "^"),
            ActorTitle = actor.Title,
            Substr = K.Functions.Substring(actor.Title, 2, 4)
          }
        );

      var joinQueryString = query.ToQueryString();

      return query
        .Subscribe(c => { Console.WriteLine($"{c.Id}: {c.ActorName} - {c.Title} - {c.ActorTitle}"); }, exception => { Console.WriteLine(exception.Message); });
    }

    private static IDisposable FullOuterJoinTables(KSqlDBContext context)
    {
      var query = context.CreateQueryStream<MovieNullableFields>("Movies")
        .FullOuterJoin(
          Source.Of<Lead_Actor>(nameof(Lead_Actor)),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            movie.Id,
            Title = movie.Title,
            movie.Release_Year,
            ActorTitle = actor.Title,
            ActorName = actor.Actor_Name
          }
        );

      var joinQueryString = query.ToQueryString();

      return query
        .Subscribe(c =>
        {
          if (c.Id.HasValue)
            Console.WriteLine($"{c.Id}: {c.ActorName} - {c.Title} - {c.ActorTitle}");
          else
            Console.WriteLine($"No movie id: {c.ActorName} - {c.Title} - {c.ActorTitle}");
        }, exception => { Console.WriteLine(exception.Message); });
    }

    private static IDisposable Window(KSqlDBContext context)
    {
      var subscription1 = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
        .Select(g => new { g.WindowStart, g.WindowEnd, Id = g.Key, Count = g.Count() })
        .Subscribe(c => { Console.WriteLine($"{c.Id}: {c.Count}: {c.WindowStart}: {c.WindowEnd}"); }, exception => { Console.WriteLine(exception.Message); });

      var query = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)).WithAdvanceBy(Duration.OfSeconds(4))
          .WithRetention(Duration.OfDays(7)))
        .Select(g => new { Id = g.Key, Count = g.Count() });

      var hoppingWindowQueryString = query.ToQueryString();

      var subscription2 = query
        .Subscribe(c => { Console.WriteLine($"{c.Id}: {c.Count}"); }, exception => { Console.WriteLine(exception.Message); });

      return new CompositeDisposable { subscription1, subscription2 };
    }

    private static async Task AsyncEnumerable(KSqlDBContext context)
    {
      var cts = new CancellationTokenSource();
      var asyncTweetsEnumerable = context.CreateQueryStream<Movie>().ToAsyncEnumerable();

      await foreach (var movie in asyncTweetsEnumerable.WithCancellation(cts.Token))
      {
        Console.WriteLine(movie.Title);

        cts.Cancel();
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

    private static async Task ToQueryStringExample(string ksqlDbUrl)
    {
      var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
      await using var context = new KSqlDBContext(contextOptions);

      var ksql = context.CreateQueryStream<Person>().ToQueryString();

      //prints SELECT * FROM People EMIT CHANGES;
      Console.WriteLine(ksql);
    }

    private static async Task GroupBy()
    {
      var ksqlDbUrl = @"http:\\localhost:8088";
      var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);

      contextOptions.QueryStreamParameters["auto.offset.reset"] = "latest";
      await using var context = new KSqlDBContext(contextOptions);

      context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Id = g.Key, Count = g.Count() })
        .Subscribe(count =>
        {
          Console.WriteLine($"{count.Id} Count: {count.Count}");
          Console.WriteLine();
        }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));


      context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => g.Count())
        .Subscribe(count =>
        {
          Console.WriteLine($"Count: {count}");
          Console.WriteLine();
        }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));

      context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Count = g.Count() })
        .Subscribe(count =>
        {
          Console.WriteLine($"Count: {count}");
          Console.WriteLine();
        }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));

      //Sum
      var subscription = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        //.Select(g => g.Sum(c => c.Id))
        .Select(g => new { Id = g.Key, MySum = g.Sum(c => c.Id) })
        .Subscribe(sum =>
        {
          Console.WriteLine($"{sum}");
          Console.WriteLine();
        }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));

      var groupBySubscription = context.CreateQueryStream<IoTSensorChange>("sqlserversensors")
        .GroupBy(c => new { c.Op, c.After.Value })
        .Select(g => new { g.Source.Op, g.Source.After.Value, num_times = g.Count() })
        .Subscribe(c =>
        {
          Console.WriteLine($"{c}");
        }, error =>
        {

        }, () => { });
    }

    private static IDisposable NotNull(KSqlDBContext context)
    {
      return context.CreateQueryStream<Click>()
        .Where(c => c.IP_ADDRESS != null)
        .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
        .Subscribe(message => Console.WriteLine(message), error => { Console.WriteLine($"Exception: {error.Message}"); });
    }

    private static IDisposable DynamicFunctionCall(KSqlDBContext context)
    {
      var ifNullQueryString = context.CreateQueryStream<Tweet>()
        .Select(c => new { c.Id, c.Amount, Col = K.F.Dynamic("IFNULL(Message, 'n/a')") as string })
        .ToQueryString();

      return context.CreateQueryStream<Tweet>()
        .Select(c => K.Functions.Dynamic("ARRAY_DISTINCT(ARRAY[1, 1, 2, 3, 1, 2])") as int[])
        .Subscribe(
          message => Console.WriteLine($"{message[0]} - {message[^1]}"),
          error => Console.WriteLine($"Exception: {error.Message}"));
    }

    private static IDisposable Having(KSqlDBContext context)
    {
      return
        //https://kafka-tutorials.confluent.io/finding-distinct-events/ksql.html
        context.CreateQueryStream<Click>()
        .GroupBy(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
        .WindowedBy(new TimeWindows(Duration.OfMinutes(2)))
        .Having(c => c.Count(g => c.Key.IP_ADDRESS) == 1)
        .Select(g => new { g.Key.IP_ADDRESS, g.Key.URL, g.Key.TIMESTAMP })
        .Take(3)
        .Subscribe(onNext: message =>
        {
          Console.WriteLine($"{nameof(Click)}: {message}");
          Console.WriteLine($"{nameof(Click)}: {message.URL} - {message.TIMESTAMP}");
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
    }

    private static IDisposable TopKDistinct(KSqlDBContext context)
    {
      return context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Id = g.Key, TopK = g.TopKDistinct(c => c.Amount, 2) })
        // .Select(g => new { Id = g.Key, TopK = g.TopK(c => c.Amount, 2) })
        .Subscribe(onNext: tweetMessage =>
        {
          var tops = string.Join(',', tweetMessage.TopK);
          Console.WriteLine($"{nameof(Tweet)} Tops: {tops}");
          Console.WriteLine($"{nameof(Tweet)}: {tweetMessage}");
          Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.TopK[0]} - {tweetMessage.TopK[^1]}");

          Console.WriteLine($"TopKs Array Length: {tops.Length}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
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

    private static IDisposable LatestByOffset(KSqlDBContext context)
    {
      var query = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Id = g.Key, EarliestByOffset = g.EarliestByOffset(c => c.Amount, 2) })
        .ToQueryString();

      return context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        //.Select(g => new { Id = g.Key, Earliest = g.EarliestByOffset(c => c.Message) })
        //.Select(g => new { Id = g.Key, Earliest = g.EarliestByOffsetAllowNulls(c => c.Message) })
        //.Select(g => new { Id = g.Key, Earliest = g.LatestByOffset(c => c.Message) })
        .Select(g => new { Id = g.Key, Earliest = g.LatestByOffsetAllowNulls(c => c.Message) })
        .Take(2) // LIMIT 2    
        .Subscribe(onNext: tweetMessage =>
        {
          Console.WriteLine($"{nameof(Tweet)}: {tweetMessage}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
    }

    private static IDisposable CollectSet(KSqlDBContext context)
    {
      var subscription = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Id = g.Key, Array = g.CollectSet(c => c.Message) })
        //.Select(g => new { Id = g.Key, Array = g.CollectList(c => c.Message) })
        .Subscribe(c =>
        {
          Console.WriteLine($"{c.Id}:");
          foreach (var value in c.Array)
          {
            Console.WriteLine($"  {value}");
          }
        }, exception => { Console.WriteLine(exception.Message); });

      return subscription;
    }

    private static IDisposable Arrays(KSqlDBContext context)
    {
      var subscription =
        context.CreateQueryStream<Tweet>()
          .Select(_ => new { FirstItem = new[] { 1, 2, 3 }[1] })
          .Subscribe(onNext: c => { Console.WriteLine($"Array first value: {c}"); },
            onError: error => { Console.WriteLine($"Exception: {error.Message}"); });

      var arrayLengthQuery = context.CreateQueryStream<Tweet>()
        .Select(_ => new[] { 1, 2, 3 }.Length)
        .ToQueryString();

      return subscription;
    }

    private static IDisposable CountDistinct(KSqlDBContext context)
    {
      var subscription = context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        // .Select(g => new { Id = g.Key, Count = g.CountDistinct(c => c.Message) })
        .Select(g => new { Id = g.Key, Count = g.LongCountDistinct(c => c.Message) })
        .Subscribe(c =>
        {
          Console.WriteLine($"{c.Id} - {c.Count}");
        }, exception => { Console.WriteLine(exception.Message); });

      return subscription;
    }

    private static IDisposable NestedTypes(KSqlDBContext context)
    {
      var disposable =
        context.CreateQueryStream<Tweet>()
          .Select(c => new
          {
            MapValue = new Dictionary<string, Dictionary<string, int>>
            {
              { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
              { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
            }["a"]
          })
          .Subscribe(
            message =>
            {
              Console.WriteLine($"Dictionary with {message.MapValue.Count} values");
              foreach (var value in message.MapValue)
              {
                Console.WriteLine($"    {value.Key} - {value.Value}");
              }
            },
            error => Console.WriteLine($"Exception: {error.Message}"));

      return disposable;
    }

    private struct MovieStruct
    {
      public string Title { get; set; }

      public int Id { get; set; }
    }

    private static async Task DeeplyNestedTypes(KSqlDBContext context)
    {
      var moviesStream = context.CreateQueryStream<Movie>();

      var source = moviesStream.Select(c => new
      {
        c.Id,
        Arr = new[]
        {
          new MovieStruct
          {
            Title = c.Title,
            Id = c.Id,
          },
          new MovieStruct
          {
            Title = "test",
            Id = 2,
          }
        },
        MapValue = new Dictionary<string, Dictionary<string, int>>
        {
          { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
          { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
        },
        MapArr = new Dictionary<int, string[]>
        {
          { 1, new[] { "a", "b"} },
          { 2, new[] { "c", "d"} }
        },
        Str = new MovieStruct { Title = c.Title, Id = c.Id },
        c.Release_Year
      }).ToAsyncEnumerable();

      await foreach (var movie in source)
      {
        Console.WriteLine($"{movie.Str.Title} - {movie.Release_Year}");
      }
    }

    private static async Task StructType(KSqlDBContext context)
    {
      var moviesStream = context.CreateQueryStream<Movie>();

      var source = moviesStream.Select(c => new
      {
        Str = new MovieStruct { Title = c.Title, Id = c.Id },
        c.Release_Year
      }).ToAsyncEnumerable();

      await foreach (var movie in source)
      {
        Console.WriteLine($"{movie.Str.Title} - {movie.Release_Year}");
      }
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

    private static async Task CreateStreamAsync()
    {
      EntityCreationMetadata metadata = new()
      {
        KafkaTopic = nameof(Movie),
        Partitions = 1,
        Replicas = 1
      };

      string url = @"http:\\localhost:8088";

      var http = new HttpClientFactory(new Uri(url));
      var restApiClient = new KSqlDbRestApiClient(http);

      var httpResponseMessage = await restApiClient.CreateStreamAsync<MovieNullableFields>(metadata, ifNotExists: true);

      //OR
      //httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<MovieNullableFields>(metadata);

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
    }

    private static async Task TerminatePersistentQueryAsync(IKSqlDbRestApiClient restApiClient)
    {
      string topicName = "moviesByTitle";

      var queries = await restApiClient.GetQueriesAsync();

      var query = queries.SelectMany(c => c.Queries).FirstOrDefault(c => c.SinkKafkaTopics.Contains(topicName));

      var response = await restApiClient.TerminatePersistentQueryAsync(query.Id);
    }

    private static async Task TerminatePushQueryAsync(IKSqlDBContext context, IKSqlDbRestApiClient restApiClient)
    {
      var subscription = await context.CreateQueryStream<Movie>()
        .SubscribeOn(ThreadPoolScheduler.Instance)
        .SubscribeAsync(onNext: _ => { }, onError: e => { }, onCompleted: () => { });

      var response = await restApiClient.TerminatePushQueryAsync(subscription.QueryId);
    }

    private static string SourceConnectorName => "mock-source-connector";
    private static string SinkConnectorName => "mock-sink-connector";

    private static async Task CreateConnectorsAsync(IKSqlDbRestApiClient restApiClient)
    {
      var sourceConnectorConfig = new Dictionary<string, string>
      {
        {"connector.class", "org.apache.kafka.connect.tools.MockSourceConnector"}
      };

      var httpResponseMessage = await restApiClient.CreateSourceConnectorAsync(sourceConnectorConfig, SourceConnectorName);

      var sinkConnectorConfig = new Dictionary<string, string> {
        { "connector.class", "org.apache.kafka.connect.tools.MockSinkConnector" },
        { "topics.regex", "mock-sink*"},
      };

      httpResponseMessage = await restApiClient.CreateSinkConnectorAsync(sinkConnectorConfig, SinkConnectorName);

      httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SinkConnectorName}`");
    }

    private static async Task CreateTypesAsync(IKSqlDbRestApiClient restApiClient)
    {
      var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
Drop type Person;
Drop type Address;
"));
      
      //Act
      httpResponseMessage = await restApiClient.CreateTypeAsync<Address>();
      httpResponseMessage = await restApiClient.CreateTypeAsync<Person>();
    }

    private static async Task SubscriptionToAComplexTypeAsync(IKSqlDbRestApiClient restApiClient, IKSqlDBContext ksqlDbContext)
    {
      string typeName = nameof(EventCategory);
      var httpResponseMessage = await restApiClient.DropTypeIfExistsAsync(typeName);

      httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop table {nameof(Event)};
"));

      httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
      httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata { KafkaTopic = "Events", Partitions = 1 });

      var subscription = ksqlDbContext.CreateQueryStream<Event>()
        .Subscribe(value =>
          {
            Console.WriteLine("Categories: ");

            foreach (var category in value.Categories ?? Enumerable.Empty<EventCategory>())
            {
              Console.WriteLine($"{category.Name}");
            }
          }, error =>
          {
            Console.WriteLine(error.Message);
          });

      httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
INSERT INTO Events (Id, Places, Categories) VALUES (1, ARRAY['Place1','Place2','Place3'], ARRAY[STRUCT(Name := 'Planet Earth'), STRUCT(Name := 'Discovery')]);"));
    }

    // await SubscriptionToAComplexTypeAsync(restApiProvider, context);
    // await InsertAComplexTypeAsync(restApiProvider, context);
    
    private static async Task InsertAComplexTypeAsync(IKSqlDbRestApiClient restApiClient, IKSqlDBContext ksqlDbContext)
    {
      var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop type {nameof(EventCategory)};
Drop table {nameof(Event)};
"));

      httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
      httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata { KafkaTopic = "Events", Partitions = 1 });

      var eventCategory = new EventCategory
      {
        Name = "Planet Earth"
      };

      var testEvent = new Event
      {
        Id = 42,
        Places = new[] { "Place1", "Place2", "Place3" },
        //Categories = new[] { eventCategory, new EventCategory { Name = "Discovery" } } // TODO
      };

      httpResponseMessage = await restApiClient.InsertIntoAsync(testEvent);
    }
    
    private static void InvocationFunctions(IKSqlDBContext ksqlDbContext)
    {
      var ksql = ksqlDbContext.CreateQueryStream<Lambda>()
        .Select(c => new
        {
          Transformed = KSqlFunctions.Instance.Transform(c.Lambda_Arr, x => x + 1),
          Filtered = KSqlFunctions.Instance.Filter(c.Lambda_Arr, x => x > 1),
          Acc = K.Functions.Reduce(c.Lambda_Arr, 0, (x, y) => x + y)
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

    private static void Bytes(IKSqlDBContext ksqlDbContext)
    {
      var ksql = ksqlDbContext.CreateQuery<Thumbnail>()
        .Select(c => new { Col = K.Functions.FromBytes(c.Image, "hex") })
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

    public static async Task Main2(string[] args)
    {
      await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostingContext, logging) =>
                          {
                            logging.AddConsole();
                            logging.AddDebug();
                          })
        .ConfigureServices((hostContext, serviceCollection) =>
                           {
                             var ksqlDbUrl = @"http:\\localhost:8088";

                             serviceCollection.ConfigureKSqlDb(ksqlDbUrl, setupParameters =>
                                                                          {
                                                                            setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);
                                                                          });

                             serviceCollection.AddHostedService<Worker>();
                           });
  }
}