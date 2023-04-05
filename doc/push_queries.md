# Push queries

### Take (LIMIT)
**v1.0.0**

Returns a specified number of contiguous elements from the start of a stream. Depends on the 'auto.topic.offset' parameter.

```C#
context.CreateQueryStream<Tweet>()
  .Take(2);
```

```SQL
SELECT *
  FROM tweets
  EMIT CHANGES
 LIMIT 2;
```

### Select
**v1.0.0**

Projects each element of a stream into a new form.
```C#
context.CreateQueryStream<Tweet>()
  .Select(l => new { l.RowTime, l.Message });
```
Omitting select is equivalent to SELECT *

- selecting of values from captured variables

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
SELECT STRUCT(Property := 42) AS Value
  FROM Locations
  EMIT CHANGES;
```

### Where
**v1.0.0**

Filters a stream of values based on a predicate.
```C#
context.CreateQueryStream<Tweet>()
  .Where(p => p.Message != "Hello world" || p.Id == 1)
  .Where(p => p.RowTime >= 1510923225000);
```
Multiple Where statements are joined with AND operator. 
```SQL
SELECT *
  FROM Tweets
 WHERE Message != 'Hello world' OR Id = 1 AND RowTime >= 1510923225000
  EMIT CHANGES;
```

List of supported operators is [documented here](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md).

### Subscribe
**v1.0.0**

Providing ```IObserver<T>```:
```C#
using var subscription = new KSqlDBContext(@"http://localhost:8088")
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
using var subscription = new KSqlDBContext(@"http://localhost:8088")
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

### ToObservable
**v1.0.0**

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

### Getting the generated KSQL

`ToQueryString` is mainly helpful for debugging purposes. It returns the generated ksql query without executing it.
```C#
var ksql = context.CreateQueryStream<Tweet>().ToQueryString();

//prints SELECT * FROM tweets EMIT CHANGES;
Console.WriteLine(ksql);
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

### SubscribeAsync
**v1.0.0**

- Subscribes an element handler, an exception handler, and a completion handler to an qbservable stream and asynchronously returns the query id.

### SubscribeOn
**v1.0.0**

- Wraps the source sequence in order to run its subscription on the specified scheduler.

### ObserveOn
**v1.0.0**

- Wraps the source sequence in order to run its observer callbacks on the specified scheduler.

```C#
using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.Sample.Models.Movies;

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

    Console.WriteLine($"Query id: {subscription}");
  }
  catch (Exception e)
  {
    Console.WriteLine(e);
  }
}
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

### ToStatementString
**v1.0.0**

Generates ksql statement from Create(OrReplace)[Table|Stream]Statements
```C#
await using var context = new KSqlDBContext(@"http://localhost:8088");

var statement = context.CreateOrReplaceTableStatement(tableName: "MoviesByTitle")
  .As<Movie>()
  .Where(c => c.Id < 3)
  .Select(c => new {c.Title, ReleaseYear = c.Release_Year})
  .PartitionBy(c => c.Title)
  .ToStatementString();
```

Generated KSQL:
```KSQL
CREATE OR REPLACE TABLE MoviesByTitle
AS SELECT Title, Release_Year AS ReleaseYear FROM Movies
WHERE Id < 3 PARTITION BY Title EMIT CHANGES;
```

### Raw string KSQL query execution

The following examples show how to execute ksql queries from strings:
```C#
string ksql = @"SELECT * FROM Movies
WHERE Title != 'E.T.' EMIT CHANGES LIMIT 2;";

QueryParameters queryParameters = new QueryParameters
{
  Sql = ksql,
  [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
};

await using var context = new KSqlDBContext(@"http://localhost:8088");

var moviesSource = context.CreateQuery<Movie>(queryParameters)
  .ToObservable();
```

Query stream:
```C#
string ksql = @"SELECT * FROM Movies
WHERE Title != 'E.T.' EMIT CHANGES LIMIT 2;";

QueryStreamParameters queryStreamParameters = new QueryStreamParameters
{
  Sql = ksql,
  [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
};

await using var context = new KSqlDBContext(@"http://localhost:8088");

var source = context.CreateQueryStream<Movie>(queryStreamParameters)
  .ToObservable();
```

### WithOffsetResetPolicy - push queries extension method
**v1.0.0**

Overrides the AutoOffsetReset policy for the current query:
```C#
var subscription = context.CreateQueryStream<Movie>()
  .WithOffsetResetPolicy(AutoOffsetReset.Latest)
  .Subscribe(movie =>
  {
    Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
  }, e => { Console.WriteLine($"Exception: {e.Message}"); });   
```

### Record (row) class
Record class is a base class for rows returned in push queries. It has a 'RowTime' property.

```C#
public class Tweet : ksqlDB.RestApi.Client.KSql.Query.Record
{
  public string Message { get; set; }
}

context.CreateQueryStream<Tweet>()
  .Select(c => new { c.RowTime, c.Message });
```
