# Pull queries

### Take (LIMIT) (v0.1.0)
Returns a specified number of contiguous elements from the start of a stream. Depends on the 'auto.topic.offset' parameter.

```C#
context.CreateQueryStream<Tweet>()
  .Take(2);
```

```SQL
SELECT * from tweets EMIT CHANGES LIMIT 2;
```

### Select
**v1.0.0**

- generation of values from captured variables

```C#
var value = new FooClass { Property = 42 };

var query = context.CreateQueryStream<Location>()
    .Select(_ => new
    {
      Value = value
    });
```

Is equivalent with:
```SQL
SELECT STRUCT(Property := 42) AS Value FROM Locations EMIT CHANGES;
```

### Subscribe (v0.1.0)
Providing ```IObserver<T>```:
```C#
using var subscription = new KSqlDBContext(@"http:\\localhost:8088")
  .CreateQueryStream<Tweet>()
  .Subscribe(new TweetsObserver());

public class TweetsObserver : System.IObserver<Tweet>
{
  public void OnNext(Tweet tweetMessage)
  {
    Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
  }

  public void OnError(Exception error)
  {
    Console.WriteLine($"{nameof(Tweet)}: {error.Message}");
  }

  public void OnCompleted()
  {
    Console.WriteLine($"{nameof(Tweet)}: completed successfully");
  }
}
```

Providing ```Action<T> onNext, Action<Exception> onError and Action onCompleted```:
```C#
using var subscription = new KSqlDBContext(@"http:\\localhost:8088")
    .CreateQueryStream<Tweet>()
    .Subscribe(
      onNext: tweetMessage =>
      {
        Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
      },
      onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, 
      onCompleted: () => Console.WriteLine("Completed")
      );
```

### ToObservable (v0.1.0)
Moving to [Rx.NET](https://github.com/dotnet/reactive)
The following code snippet shows how to observe messages on the desired [IScheduler](http://introtorx.com/Content/v1.0.10621.0/15_SchedulingAndThreading.html): 

```C#
using var disposable = context.CreateQueryStream<Tweet>()        
  .Take(2)     
  .ToObservable() //client side processing starts here lazily after subscription
  .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
  .Subscribe(new TweetsObserver());
```
Be cautious regarding to server side and client side processings:
```C#
KSql.Linq.IQbservable<Tweet> queryStream = context.CreateQueryStream<Tweet>().Take(2);

System.IObservable<Tweet> observable = queryStream.ToObservable();

//All reactive extension methods are available from this point.
//The not obvious difference is that the processing is done client side, not server side (ksqldb) as in the case of IQbservable:
observable.Throttle(TimeSpan.FromSeconds(3)).Subscribe();
```
WPF client side batching example:
```C#
private static IDisposable ClientSideBatching(KSqlDBContext context)
{
  var disposable = context.CreateQueryStream<Tweet>()
    //Client side execution
    .ToObservable()
    .Buffer(TimeSpan.FromMilliseconds(250), 100)
    .Where(c => c.Count > 0)
    .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
    .Subscribe(tweets =>
    {
      foreach (var tweet in tweets)
      {
        Console.WriteLine(tweet.Message);
      }
    });

  return disposable;
}
```

### Query comprehension syntax
Note that ksqldb does not support OrderBy

```C#
var grouping = 
  from city in context.CreateQueryStream<City>()
  where city.RegionCode != "xy"
  group city by city.State.Name into g
  select new
  {
    g.Source.RegionCode,
    g.Source.State.Name,
    Num_Times = g.Count()
  };
```

### JsonPropertyNameAttribute
**v2.2.1**

- Renaming of stream or table column names with the `JsonPropertyNameAttribute` was also added for selects

### IKSqlGrouping.Source
**v1.0.0**

- grouping by nested properies. Can be used in the following way:

```C#
var source = Context.CreateQueryStream<City>()
  .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
  .GroupBy(c => new { c.RegionCode, c.State.Name })
  .Select(g => new { g.Source.RegionCode, g.Source.State.Name, Count = g.Count()})
  .Take(1)
  .ToAsyncEnumerable();
```

```C#
record City
{
  [Key]
  public string RegionCode { get; init; }
  public State State { get; init; }
}

record State
{
  public string Name { get; init; }
}
```

Equivalent KSQL:
```SQL
SELECT RegionCode, State->Name, COUNT(*) Count 
FROM Cities 
GROUP BY RegionCode, State->Name 
EMIT CHANGES;
```

### ToAsyncEnumerable
**v1.0.0**

Creates an [async iterator](https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8) from the query:
```C#
var cts = new CancellationTokenSource();
var asyncTweetsEnumerable = context.CreateQueryStream<Tweet>().ToAsyncEnumerable();

await foreach (var tweet in asyncTweetsEnumerable.WithCancellation(cts.Token))
  Console.WriteLine(tweet.Message);
```

### ExplainAsync
**v1.0.0**

- `ExplainAsync` - Show the execution plan for a SQL expression, show the execution plan plus additional runtime information and metrics.

```C#
using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;
using ksqlDB.RestApi.Client.Sample.Models.Movies;

public static async Task ExplainAsync(IKSqlDBContext context)
{
  var query = context.CreateQueryStream<Movie>()
    .Where(c => c.Title != "E.T.");

  string explain = await query
    .ExplainAsStringAsync();

  ExplainResponse[] explainResponses = await context.CreateQueryStream<Movie>().ExplainAsync();
      
  Console.WriteLine(explainResponses[0].QueryDescription.ExecutionPlan);
}
```
