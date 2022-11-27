# v1.7.0

# Aggregation functions

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

### COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET - with Structs, Arrays, and Maps

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
