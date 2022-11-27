# v1.7.0

# Aggregate functions COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET - with Structs, Arrays, and Maps

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