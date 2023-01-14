using Aggregations.Model;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using System.Reactive.Disposables;
using Aggregations.KSqlDbContext;
using ksqlDb.RestApi.Client.KSql.Query.PushQueries;

const string ksqlDbUrl = @"http:\\localhost:8088";

var host = Host.CreateDefaultBuilder(args)
  .ConfigureLogging((hostingContext, logging) =>
  {
    logging.AddConsole();
    logging.AddDebug();
  })
  .ConfigureServices((_, serviceCollection) =>
  {
    serviceCollection.AddDbContext<IApplicationKSqlDbContext, ApplicationKSqlDbContext>(
      options =>
      {
        var setupParameters = options.UseKSqlDb(ksqlDbUrl);

        setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

      }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

    serviceCollection.AddDbContextFactory<IApplicationKSqlDbContext>(factoryLifetime: ServiceLifetime.Scoped);
  }).Build();

await using var context = host.Services.GetRequiredService<IApplicationKSqlDbContext>();

using var disposable = LatestByOffset(context);

await host.RunAsync();

static IDisposable LatestByOffset(IApplicationKSqlDbContext context)
{
  var query = context.Tweets
    .GroupBy(c => c.Id)
    .Select(g => new { Id = g.Key, EarliestByOffset = g.EarliestByOffset(c => c.Amount, 2) })
    .ToQueryString();

  Console.WriteLine(query);

  return context.Tweets
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

#pragma warning disable CS8321 // Local function is declared but never used

static IDisposable Having(IKSqlDBContext context)
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

static async Task GroupBy()
{
  var ksqlDbUrl = @"http:\\localhost:8088";
  var contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
  {
    QueryStreamParameters =
    {
      ["auto.offset.reset"] = "latest"
    }
  };

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
    .GroupBy(c => new { c.Op, c.After!.Value })
    .Select(g => new { g.Source.Op, g.Source.After!.Value, num_times = g.Count() })
    .Subscribe(c =>
    {
      Console.WriteLine($"{c}");
    }, error =>
    {

    }, () => { });
}

static IDisposable Window(IKSqlDBContext context)
{
  new TimeWindows(Duration.OfSeconds(2), OutputRefinement.Final).WithGracePeriod(Duration.OfSeconds(2));

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

static IDisposable CountDistinct(IKSqlDBContext context)
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

static IDisposable CollectSet(IKSqlDBContext context)
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

static IDisposable TopKDistinct(IKSqlDBContext context)
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

static void EmitFinal(IKSqlDBContext ksqlDbContext)
{
  var tumblingWindow =
    new TimeWindows(Duration.OfSeconds(2), OutputRefinement.Final).WithGracePeriod(Duration.OfSeconds(2));

  var query = ksqlDbContext.CreateQueryStream<Tweet>()
    .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
    .GroupBy(c => c.Id)
    .WindowedBy(tumblingWindow)
    .Select(g => new { Id = g.Key, Count = g.Count(c => c.Message) })
    .ToQueryString();
}

#pragma warning restore CS8321 // Local function is declared but never used
