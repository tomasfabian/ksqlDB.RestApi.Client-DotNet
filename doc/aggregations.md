# Aggregation functions
[Rest api reference](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

SQL **aggregation functions** are built-in functions that operate on a set of values from a column of a database table and return a single, aggregated value.
These functions are commonly used in SQL queries to perform calculations on groups of data or to summarize data.

In a **streaming database** such as `ksqlDB` the concept of SQL aggregation functions is similar, but there are some differences due to the nature of streaming data and the capabilities of the streaming database.
In `ksqlDB`, you can use aggregation functions to perform calculations and transformations on streaming data.
`ksqlDB` also provides additional features for working with streaming data, such as **windowing** and **time-based** operations, which allow you to aggregate data over specified time intervals.

### GroupBy
Group records in a window. Required by the WINDOW clause. Windowing queries must group by the keys that are selected in the query.

### Count
**v1.0.0**

Count the number of rows. When * is specified, the count returned will be the total number of rows.
```C#
var ksqlDbUrl = @"http://localhost:8088";
var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);
var context = new KSqlDBContext(contextOptions);

context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => new { Id = g.Key, Count = g.Count() })
  .Subscribe(count =>
  {
    Console.WriteLine($"{count.Id} Count: {count.Count}");
    Console.WriteLine();
  }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));
```
```SQL
SELECT Id, COUNT(*) Count
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

> ⚠ There is a known limitation in the early access versions (bellow version 1.10). The aggregation functions have to be named/aliased COUNT(*) Count, otherwise the deserialization won't be able to map the unknown column name KSQL_COL_0. 
The Key should be mapped back to the respective column too Id = g.Key. See IKSqlGrouping.Source (v1.10.0).

Or without the new expression:
```C#
context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => g.Count()); 
```
```SQL
SELECT COUNT(*)
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

### Having
**v1.0.0**

Extract records from an aggregation that fulfill a specified condition.

```C#
var query = context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Having(c => c.Count() > 2)
  .Select(g => new { Id = g.Key, Count = g.Count()});
```
KSQL:
```KSQL
SELECT Id, COUNT(*) Count
  FROM Tweets
 GROUP BY Id
HAVING Count(*) > 2
  EMIT CHANGES;
```

### Having - aggregations with a column
[Example](https://kafka-tutorials.confluent.io/finding-distinct-events/ksql.html) shows how to use Having with Count(column) and GroupBy compound key:
```C#
public class Click
{
  public string IP_ADDRESS { get; set; }
  public string URL { get; set; }
  public string TIMESTAMP { get; set; }
}

var query = context.CreateQueryStream<Click>()
  .GroupBy(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
  .WindowedBy(new TimeWindows(Duration.OfMinutes(2)))
  .Having(c => c.Count(g => c.Key.IP_ADDRESS) == 1)
  .Select(g => new { g.Key.IP_ADDRESS, g.Key.URL, g.Key.TIMESTAMP })
  .Take(3);
```

Generated KSQL:
```SQL
SELECT IP_ADDRESS, URL, TIMESTAMP FROM Clicks WINDOW TUMBLING (SIZE 2 MINUTES)
 GROUP BY IP_ADDRESS, URL, TIMESTAMP 
HAVING COUNT(IP_ADDRESS) = 1
  EMIT CHANGES
 LIMIT 3;
```

### Sum

Sums the column values.

```C#
context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        //.Select(g => g.Sum(c => c.Amount))
        .Select(g => new { Id = g.Key, Agg = g.Sum(c => c.Amount)})
```
Equivalent to KSql:
```SQL
SELECT Id, SUM(Amount) Agg
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

### Avg
**v1.0.0**

Return the average value for a given column.
```C#
var query = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Avg(c => c.Citizens));
```

```KSQL
AVG(col1)
``` 

### Min and Max
**v1.0.0**

Return the minimum/maximum value for a given column and window. Rows that have col1 set to null are ignored.
```C#
var queryMin = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Min(c => c.Citizens));

var queryMax = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Max(c => c.Citizens));
```

```KSQL
MIN(col1)
MAX(col1)
``` 

### COLLECT_LIST, COLLECT_SET
**v1.0.0**
- **COLLECT_LIST** - returns an array containing all the values of col1 from each input row (for the specified grouping and time window, if any).
- **COLLECT_SET** - returns an array containing the distinct values of col1 from each input row (for the specified grouping and time window, if any).

- with Structs, Arrays, and Maps

The list of available `kslqdb` aggregate functions is available [here](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

```C#
var dict = new Dictionary<string, int>()
{
  ["Karen"] = 3,
  ["Thomas"] = 42,
};

var source = Context.CreateQueryStream<Tweet>(TweetsStreamName)
  .GroupBy(c => c.Id)
  .Select(l => new { Id = l.Key, Maps = l.CollectList(c => dict) })
```

### TopK, TopKDistinct, LongCount, Count(column
**v1.0.0**

```C#
Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression1 = l => new { TopK = l.TopK(c => c.Amount, 2) };
Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression2 = l => new { TopKDistinct = l.TopKDistinct(c => c.Amount, 2) };
Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression3 = l => new { Count = l.LongCount(c => c.Amount) };
```

KSQL
```KSQL
TOPK(Amount, 2) TopKDistinct
TOPKDISTINCT(Amount, 2) TopKDistinct
COUNT(Amount) Count
```

```C#
new KSqlDBContext(@"http://localhost:8088").CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => new { Id = g.Key, TopK = g.TopKDistinct(c => c.Amount, 4) })
  .Subscribe(onNext: tweetMessage =>
  {
    var tops = string.Join(',', tweetMessage.TopK);
    Console.WriteLine($"TopKs: {tops}");
    Console.WriteLine($"TopKs Array Length: {tops.Length}");
  }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
```

### EarliestByOffset, LatestByOffset, EarliestByOffsetAllowNulls, LatestByOffsetAllowNull

- [EarliestByOffset](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/#earliest_by_offset) - returns the earliest value for the specified column.
- [LatestByOffset](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/#latest_by_offset) - returns the latest value for the specified column.

```C#
Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression1 = l => new { EarliestByOffset = l.EarliestByOffset(c => c.Amount) };

Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression2 = l => new { LatestByOffsetAllowNulls = l.LatestByOffsetAllowNulls(c => c.Amount) };
```
KSQL
```KSQL
EARLIEST_BY_OFFSET(col1, [ignoreNulls])
```
```KSQL
EARLIEST_BY_OFFSET(Amount, True) EarliestByOffset
LATEST_BY_OFFSET(Amount, False) LatestByOffsetAllowNulls
```

```KSQL
EARLIEST_BY_OFFSET(col1, earliestN, [ignoreNulls])
```

Return the earliest N values for the specified column as an ARRAY. The earliest values
in the partition have the lowest offsets.
```C#
await using var context = new KSqlDBContext(@"http://localhost:8088");

context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => new { Id = g.Key, EarliestByOffset = g.EarliestByOffset(c => c.Amount, 2) })
  .Subscribe(earliest =>
  {
    Console.WriteLine($"{earliest.Id} array length: {earliest.EarliestByOffset.Length}");
  }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));
```

Generated KSQL:
```SQL
SELECT Id, EARLIEST_BY_OFFSET(Amount, 2, True) EarliestByOffset 
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

### TimeWindows - EMIT FINAL
**v2.5.0**

- `EMIT FINAL` output refinement was added for windowed aggregations. ksqldb v0.28.2

```C#
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.KSql.Query.PushQueries;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

var tumblingWindow =
  new TimeWindows(Duration.OfSeconds(2), OutputRefinement.Final).WithGracePeriod(Duration.OfSeconds(2));

var query = Context.CreateQueryStream<Tweet>()
  .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
  .GroupBy(c => c.Id)
  .WindowedBy(tumblingWindow)
  .Select(g => new { Id = g.Key, Count = g.Count(c => c.Message) })
  .ToQueryString()
```

```SQL
SELECT Id, COUNT(MESSAGE) Count
  FROM tweets
WINDOW TUMBLING (SIZE 2 SECONDS, GRACE PERIOD 2 SECONDS)
 GROUP BY Id EMIT FINAL;
```

### CollectSet, CollectList, CountDistinct
**v1.0.0**

```C#
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
```
Generated KSQL:
```SQL
SELECT Id, COLLECT_SET(Message) Array 
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;

SELECT Id, COLLECT_LIST(Message) Array 
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

CountDistinct, LongCountDistinct
```C#
var subscription = context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  // .Select(g => new { Id = g.Key, Count = g.CountDistinct(c => c.Message) })
  .Select(g => new { Id = g.Key, Count = g.LongCountDistinct(c => c.Message) })
  .Subscribe(c =>
  {
    Console.WriteLine($"{c.Id} - {c.Count}");
  }, exception => { Console.WriteLine(exception.Message); });
```

Generated KSQL:
```SQL
SELECT Id, COUNT_DISTINCT(Message) Count 
  FROM Tweets
 GROUP BY Id
  EMIT CHANGES;
```

## WindowedBy
**v1.0.0**

Creation of windowed aggregation

### Tumbling window
[Tumbling window](https://docs.ksqldb.io/en/latest/concepts/time-and-windows-in-ksqldb-queries/#tumbling-window) is a time-based windowing mechanism used for aggregating and processing streaming data within **fixed**, non-overlapping time intervals.

```C#
var context = new TransactionsDbProvider(ksqlDbUrl);

var windowedQuery = context.CreateQueryStream<Transaction>()
  .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
  .GroupBy(c => c.CardNumber)
  .Select(g => new { CardNumber = g.Key, Count = g.Count() });
```

```SQL
SELECT CardNumber, COUNT(*) Count
  FROM Transactions 
WINDOW TUMBLING (SIZE 5 SECONDS, GRACE PERIOD 2 HOURS) 
 GROUP BY CardNumber
  EMIT CHANGES;
```

### Hopping window

[Hopping window](https://docs.ksqldb.io/en/latest/concepts/time-and-windows-in-ksqldb-queries/#hopping-window) is a time-based windowing mechanism used for aggregating and processing streaming data within **overlapping** time intervals.
```C#
var subscription = context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)).WithAdvanceBy(Duration.OfSeconds(4)).WithRetention(Duration.OfDays(7)))
  .Select(g => new { g.WindowStart, g.WindowEnd, Id = g.Key, Count = g.Count() })
  .Subscribe(c => { Console.WriteLine($"{c.Id}: {c.Count}: {c.WindowStart}: {c.WindowEnd}"); }, exception => {});
```

```SQL
SELECT WindowStart, WindowEnd, Id, COUNT(*) Count
  FROM Tweets 
WINDOW HOPPING (SIZE 5 SECONDS, ADVANCE BY 10 SECONDS, RETENTION 7 DAYS) 
 GROUP BY Id
  EMIT CHANGES;
```
Window advancement interval should be more than zero and less than window duration

### Session Window
**v1.0.0**

A [session window](https://docs.ksqldb.io/en/latest/concepts/time-and-windows-in-ksqldb-queries/#session-window) aggregates records into a session, which represents a period of activity separated by a specified gap of inactivity, or "idleness". 
```C#
var query = context.CreateQueryStream<Transaction>()
  .GroupBy(c => c.CardNumber)
  .WindowedBy(new SessionWindow(Duration.OfSeconds(5)))
  .Select(g => new { CardNumber = g.Key, Count = g.Count() });
```

KSQL:
```SQL
SELECT CardNumber, COUNT(*) Count
  FROM Transactions 
WINDOW SESSION (5 SECONDS)
 GROUP BY CardNumber 
  EMIT CHANGES;
```

Time units:
```C#
using ksqlDB.RestApi.Client.KSql.Query.Windows;

public enum TimeUnits
{
  MILLISECONDS, // v2.0.0
  SECONDS,
  MINUTES,
  HOURS,
  DAYS
}

Duration duration = Duration.OfHours(2);

Console.WriteLine($"{duration.Value} {duration.TimeUnit}");
```

