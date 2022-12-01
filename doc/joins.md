# JOIN clause - Joining collections

### Join within
**v1.3.0**

- specifies a time window for stream-stream joins

```C#
var query = from o in KSqlDBContext.CreateQueryStream<Order>()
  join p in Source.Of<Payment>().Within(Duration.OfHours(1), Duration.OfDays(5)) on o.OrderId equals p.Id
  select new
         {
           orderId = o.OrderId,
           paymentId = p.Id
         };
```

```SQL
SELECT o.OrderId AS orderId, p.Id AS paymentId FROM Orders o
INNER JOIN Payments p
WITHIN (1 HOURS, 5 DAYS) ON o.OrderId = p.Id
EMIT CHANGES;
```

### IKSqlDBContext Add and SaveChangesAsync
**v1.3.0**

With IKSqlDBContext.Add and IKSqlDBContext.SaveChangesAsync you can add multiple entities to the context and save them asynchronously in one request (as "batch inserts").

```C#
private static async Task AddAndSaveChangesAsync(IKSqlDBContext context)
{
  context.Add(new Movie { Id = 1 });
  context.Add(new Movie { Id = 2 });

  var saveResponse = await context.SaveChangesAsync();
}
```

### RightJoin

**v2.1.0**

- Select all records for the right side of the join and the matching records from the left side. If the matching records on the left side are missing, the corresponding columns will contain null values.

```C#
using ksqlDB.RestApi.Client.KSql.Linq;

var query = KSqlDBContext.CreateQueryStream<Lead_Actor>(ActorsTableName)
  .RightJoin(
    Source.Of<Movie>(MoviesTableName),
    actor => actor.Title,
    movie => movie.Title,
    (actor, movie) => new
    {
      movie.Id,
      Title = movie.Title,
      movie.Release_Year,
      Substr = K.Functions.Substring(movie.Title, 2, 4),
      ActorTitle = actor.Title,
    }
  ));
```

```SQL
SELECT movie.Id Id, movie.Title Title, movie.Release_Year Release_Year, SUBSTRING(movie.Title, 2, 4) Substr, actor.Title AS ActorTitle FROM lead_actor_test actor
 RIGHT JOIN movies_test movie
    ON actor.Title = movie.Title
  EMIT CHANGES;
```

### Support explicit message types for Protobuf with multiple definitions

**v2.1.0**

- the following 2 new fields were added to `CreationMetadata`: `KeySchemaFullName` and `ValueSchemaFullName`

```C#
var creationMetadata = new CreationMetadata
{
  KeySchemaFullName = "ProductKey"
  ValueSchemaFullName = "ProductInfo"
};
```
