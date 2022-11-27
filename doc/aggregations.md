# Aggregation functions
List of supported ksqldb [aggregation functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md):
- [MIN, MAX](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#min-and-max-v020)
- [AVG](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#avg-v020)
- [COUNT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#count-v010)
- [COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#collect_list-collect_set-earliest_by_offset-latest_by_offset)
- [SUM](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#sum)
- COUNT_DISTINCT
- HISTOGRAM
- [TOPK,TOPKDISTINCT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#topk-topkdistinct-longcount-countcolumn-v030)

[Rest api reference](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

### GroupBy (v0.1.0)
#### Count (v0.1.0)
Count the number of rows. When * is specified, the count returned will be the total number of rows.
```C#
var ksqlDbUrl = @"http:\\localhost:8088";
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
SELECT Id, COUNT(*) Count FROM Tweets GROUP BY Id EMIT CHANGES;
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
SELECT COUNT(*) FROM Tweets GROUP BY Id EMIT CHANGES;
```

### Having (v0.2.0)
Extract records from an aggregation that fulfill a specified condition.

```C#
var query = context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Having(c => c.Count() > 2)
  .Select(g => new { Id = g.Key, Count = g.Count()});
```
KSQL:
```KSQL
SELECT Id, COUNT(*) Count FROM Tweets GROUP BY Id HAVING Count(*) > 2 EMIT CHANGES;
```

#### Sum
```C#
context.CreateQueryStream<Tweet>()
        .GroupBy(c => c.Id)
        //.Select(g => g.Sum(c => c.Amount))
        .Select(g => new { Id = g.Key, Agg = g.Sum(c => c.Amount)})
```
Equivalent to KSql:
```SQL
SELECT Id, SUM(Amount) Agg FROM Tweets GROUP BY Id EMIT CHANGES;
```

### Avg (v0.2.0)
```KSQL
AVG(col1)
``` 
Return the average value for a given column.
```C#
var query = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Avg(c => c.Citizens));
```

### Min and Max (v0.2.0)
```KSQL
MIN(col1)
MAX(col1)
``` 
Return the minimum/maximum value for a given column and window. Rows that have col1 set to null are ignored.
```C#
var queryMin = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Min(c => c.Citizens));

var queryMax = CreateQbservable()
  .GroupBy(c => c.RegionCode)
  .Select(g => g.Max(c => c.Citizens));
```

### COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET
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

### TopK, TopKDistinct, LongCount, Count(column) (v0.3.0)
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
new KSqlDBContext(@"http:\\localhost:8088").CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => new { Id = g.Key, TopK = g.TopKDistinct(c => c.Amount, 4) })
  .Subscribe(onNext: tweetMessage =>
  {
    var tops = string.Join(',', tweetMessage.TopK);
    Console.WriteLine($"TopKs: {tops}");
    Console.WriteLine($"TopKs Array Length: {tops.Length}");
  }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
```

### EarliestByOffset, LatestByOffset, EarliestByOffsetAllowNulls, LatestByOffsetAllowNull (v0.3.0)
[EarliestByOffset](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/#earliest_by_offset),
[LatestByOffset](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/#latest_by_offset)
```C#
Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression1 = l => new { EarliestByOffset = l.EarliestByOffset(c => c.Amount) };

Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression2 = l => new { LatestByOffsetAllowNulls = l.LatestByOffsetAllowNulls(c => c.Amount) };
```
KSQL
```KSQL
--EARLIEST_BY_OFFSET(col1, [ignoreNulls])
EARLIEST_BY_OFFSET(Amount, True) EarliestByOffset
LATEST_BY_OFFSET(Amount, False) LatestByOffsetAllowNulls
```

EARLIEST_BY_OFFSET(col1, earliestN, [ignoreNulls])

Return the earliest N values for the specified column as an ARRAY. The earliest values
in the partition have the lowest offsets.
```C#
await using var context = new KSqlDBContext(@"http:\\localhost:8088");

context.CreateQueryStream<Tweet>()
  .GroupBy(c => c.Id)
  .Select(g => new { Id = g.Key, EarliestByOffset = g.EarliestByOffset(c => c.Amount, 2) })
  .Subscribe(earliest =>
  {
    Console.WriteLine($"{earliest.Id} array length: {earliest.EarliestByOffset.Length}");
  }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));
```
Generated KSQL:
```KSQL
SELECT Id, EARLIEST_BY_OFFSET(Amount, 2, True) EarliestByOffset 
FROM Tweets GROUP BY Id EMIT CHANGES;
```
