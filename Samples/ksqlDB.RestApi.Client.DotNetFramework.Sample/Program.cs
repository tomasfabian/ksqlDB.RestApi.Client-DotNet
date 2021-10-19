using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ksqlDB.Api.Client.Samples.Models;
using ksqlDB.Api.Client.Samples.Models.Movies;
using ksqlDB.Api.Client.Samples.Providers;
using ksqlDB.Api.Client.Samples.PullQuery;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace Kafka.DotNetFramework.ksqlDB.Sample
{
  class Program
  {
    public static KSqlDBContextOptions CreateQueryStreamOptions(string ksqlDbUrl)
    {
      var contextOptions = new KSqlDbContextOptionsBuilder()
        .UseKSqlDb(ksqlDbUrl)
        .SetupQuery(options =>
        {
          options.Properties[QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Latest.ToString().ToLower(); // "latest"
        })
        .Options;

      return contextOptions;
    }

    public static async Task Main(string[] args)
    {
      var ksqlDbUrl = @"http:\\localhost:8088";
      
      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
      var restApiProvider = new KSqlDbRestApiProvider(httpClientFactory);
      var moviesProvider = new MoviesProvider(restApiProvider);

      await moviesProvider.CreateTablesAsync();
      
      var contextOptions = CreateQueryStreamOptions(ksqlDbUrl);

      var context = new KSqlDBContext(contextOptions);

      var subscription = context.CreateQuery<Movie>()
        .Where(p => p.Title != "E.T.")
        .Where(c => K.Functions.Like(c.Title.ToLower(), "%hard%".ToLower()) || c.Id == 1)
        .Where(p => p.RowTime >= 1510923225000) //AND RowTime >= 1510923225000
        .Select(l => new { Id2 = l.Id, l.Title, l.Release_Year, l.RowTime })
        .Take(2) // LIMIT 2    
        .ToObservable() // client side processing starts here lazily after subscription. Switches to Rx.NET
        .ObserveOn(TaskPoolScheduler.Default)
        .Subscribe(onNext: movie =>
        {
          Console.WriteLine($"{nameof(Movie)}: {movie.Id2} - {movie.Title} - {movie.RowTime}");
          Console.WriteLine();
        }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));

      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);
      await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor1);

      try
      {
        await new PullQueryExample().ExecuteAsync();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }

      Console.WriteLine("Press any key to stop the subscription");

      Console.ReadKey();

      await context.DisposeAsync();
      using (subscription)
      {
      }

      Console.WriteLine("Subscription completed");
    }

    private static IDisposable CountDistinct(KSqlDBContext context)
    {
      var subscription = context.CreateQuery<Tweet>()
        .GroupBy(c => c.Id)
        .Select(g => new { Id = g.Key, Count = g.LongCountDistinct(c => c.Message) })
        .Subscribe(c =>
        {
          Console.WriteLine($"{c.Id} - {c.Count}");
        }, exception => { Console.WriteLine(exception.Message); });

      return subscription;
    }

    private static IDisposable Entries(KSqlDBContext context)
    {
      bool sorted = true;

      var subscription = context.CreateQuery<Movie>()
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

    private struct MovieStruct
    {
      public string Title { get; set; }

      public int Id { get; set; }
    }
    
    private static async Task DeeplyNestedTypes(KSqlDBContext context)
    {
      var moviesStream = context.CreateQuery<Movie>();

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
  }
}