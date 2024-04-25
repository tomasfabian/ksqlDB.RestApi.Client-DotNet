# Push queries

**Push queries** in `ksqlDB` provide a way to obtain continuous updates as new data arrives and matches the specified criteria.
They don't rely on batch processing or waiting for a predefined interval to produce query results.

It is important to note that `ksqlDB` does not support the **ORDER BY** clause. `ksqlDB` processes data in a streaming manner, and the order of events is based on their **arrival time** rather than explicit sorting.

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

List of supported operators is [documented here](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md).

### Subscribe
**v1.0.0**

**Subscribe** refers to the action of a client or consumer connecting to a query result stream and **receiving** continuous updates as new data arrives or the state of the underlying stream or table changes.

When you subscribe to a push query in `ksqlDB.RestApi.Client`, you establish a connection between the client application and the `ksqlDB` server, enabling the client to receive and process the continuously pushed query results.

Implementing the `IObserver<T>` interface:

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

The **Observer pattern** promotes loose coupling and enables efficient communication between objects. It allows the **subject** to **broadcast** changes to multiple observers without having explicit knowledge of their existence or specific implementations.
Observers can **react to changes** in the subject's state and perform actions accordingly.

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

Moving to [Rx.NET](https://github.com/dotnet/reactive)...

The following code snippet shows how to observe messages on the desired [IScheduler](http://introtorx.com/Content/v1.0.10621.0/15_SchedulingAndThreading.html): 

```C#
using var disposable = context.CreateQueryStream<Tweet>()        
  .Take(2)     
  .ToObservable() //client side processing starts here lazily after subscription
  .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
  .Subscribe(new TweetsObserver());
```

The `IScheduler` interface is part of the Reactive Extensions (Rx) library, which provides a set of powerful tools and abstractions for working with asynchronous and event-based programming. The `IScheduler` interface represents a scheduler that is responsible for controlling the execution and timing of Rx operations.

Be cautious regarding to server side and client side processings:
```C#
KSql.Linq.IQbservable<Tweet> queryStream = context.CreateQueryStream<Tweet>().Take(2);

System.IObservable<Tweet> observable = queryStream.ToObservable();

//All reactive extension methods are available from this point.
//One subtle distinction is that the processing occurs on the client side rather than the server side (ksqlDB) as seen in the case of IQbservable:
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

The above example **buffers** the events emitted by the observable sequence. The buffer is filled either when a time window of 250 milliseconds has passed or when 100 events have been received, whichever occurs first.

It also **filters** the buffered events, allowing only those buffers with a count greater than 0 to pass through. This ensures that we process only non-empty buffers.

`ObserveOn` switches the execution context of the observable sequence to the current `DispatcherScheduler`. This is typically used in UI applications to ensure that the subscriber code runs on the **UI thread** for proper interaction with the user interface.

### Getting the generated KSQL

`ToQueryString` is mainly helpful for debugging purposes. It returns the generated ksql query without executing it.
```C#
var ksql = context.CreateQueryStream<Tweet>().ToQueryString();

//prints SELECT * FROM tweets EMIT CHANGES;
Console.WriteLine(ksql);
```

### Query comprehension syntax

**Query comprehension syntax** provides a more declarative and readable way to express LINQ queries compared to method chaining syntax.
However, it's worth noting that query comprehension syntax is just a different way of writing LINQ queries and is ultimately translated to method calls behind the scenes.

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

Renaming of stream or table column names with the `JsonPropertyNameAttribute` was also added for selects

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

The aforementioned usage of the **await foreach** statement is employed to iterate asynchronously over the elements of the sequence generated by the async iterator.

### SubscribeAsync
**v1.0.0**

Subscribes an element handler, an exception handler, and a completion handler to an qbservable stream and asynchronously returns the query id.

### SubscribeOn
**v1.0.0**

Wraps the source sequence in order to run its subscription on the specified scheduler.

### ObserveOn
**v1.0.0**

Wraps the source sequence in order to run its observer callbacks on the specified scheduler.

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

The `ToStatementString` function is a method that converts an expression into its corresponding SQL statement string representation.

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

The following example shows the execution of string-based KSQL statements:

```C#
string ksql = @"SELECT * FROM Movies
WHERE Title != 'E.T.' EMIT CHANGES LIMIT 2;";

QueryParameters queryParameters = new QueryParameters
{
  Sql = ksql,
  [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
};

await using var context = new KSqlDBContext(@"http://localhost:8088");

var moviesSource = context.CreateQueryStream<Movie>(queryParameters)
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

In `ksqlDB`, the `AUTO_OFFSET_RESET` property is used to configure how a `ksqlDB` application should handle the offset (position) of a consumer when it starts reading from a Kafka topic or stream. It determines what to do when the consumer does not have a valid or existing offset for the topic or stream it wants to read.

The AutoOffsetReset property can have two possible values:

- **Earliest**: If set to `Earliest`, the consumer will start reading from the earliest available offset in the topic or stream. This means it will read all the messages from the beginning of the topic or stream, including any messages that were produced before the consumer started.

- **Latest**: If set to `Latest`, the consumer will start reading from the latest offset in the topic or stream. This means it will only read messages that are produced after the consumer starts, ignoring any messages that were produced prior to the consumer's start.

Overrides the AutoOffsetReset policy for the current query:
```C#
using ksqlDB.RestApi.Client.KSql.Query.Options;

var subscription = context.CreateQueryStream<Movie>()
  .WithOffsetResetPolicy(AutoOffsetReset.Latest)
  .Subscribe(movie =>
  {
    Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
  }, e => { Console.WriteLine($"Exception: {e.Message}"); });   
```
