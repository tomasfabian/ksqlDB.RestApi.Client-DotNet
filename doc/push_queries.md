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
