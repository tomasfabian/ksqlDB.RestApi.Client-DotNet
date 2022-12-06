# JOIN clause - Joining collections

### Multiple joins with query comprehension syntax (GroupJoin, SelectMany, DefaultIfEmpty)
**v1.1.0**

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

class Order
{
  public int OrderId { get; set; }
  public int PaymentId { get; set; }
  public int ShipmentId { get; set; }
}

class Payment
{
  [Key]
  public int Id { get; set; }
}

record Shipment
{
  [Key]
  public int? Id { get; set; }
}
```

```C#
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
```

```C#
var ksqlDbUrl = @"http:\\localhost:8088";

var context = new KSqlDBContext(ksqlDbUrl);

var query = (from o in context.CreateQueryStream<Order>()
    join p1 in Source.Of<Payment>() on o.PaymentId equals p1.Id
    join s1 in Source.Of<Shipment>() on o.ShipmentId equals s1.Id into gj
    from sa in gj.DefaultIfEmpty()
    select new
           {
             orderId = o.OrderId,
             shipmentId = sa.Id,
             paymentId = p1.Id,
           })
  .Take(5);
```

Equivalent KSQL:

```SQL
SELECT o.OrderId AS orderId, sa.Id AS shipmentId, p1.Id AS paymentId FROM Orders o
INNER JOIN Payments p1
ON O.PaymentId = p1.Id
LEFT JOIN Shipments sa
ON o.ShipmentId = sa.Id
EMIT CHANGES LIMIT 5;
```

Creation of entities for the above mentioned query:

```C#
var entityCreationMetadata = new EntityCreationMetadata
                             {
                               KafkaTopic = nameof(Order) + "-Join",
                               Partitions = 1
                             };

var response = await restApiClient.CreateStreamAsync<Order>(entityCreationMetadata, ifNotExists: true);
response = await restApiClient.CreateTableAsync<Payment>(entityCreationMetadata with { KafkaTopic = nameof(Payment) }, ifNotExists: true);
response = await restApiClient.CreateTableAsync<Shipment>(entityCreationMetadata with { KafkaTopic = nameof(Shipment) }, ifNotExists: true);
```

Listen to the incoming record messages:

```C#
using var subscription = query
  .Subscribe(c => {
               Console.WriteLine($"{nameof(Order.OrderId)}: {c.orderId}");

               Console.WriteLine($"{nameof(Order.PaymentId)}: {c.paymentId}");

               if (c.shipmentId.HasValue)
                 Console.WriteLine($"{nameof(Order.ShipmentId)}: {c.shipmentId}");

             }, error => {
                  Console.WriteLine(error.Message);
                });
```

Inserting of sample data:

```C#
var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
var payment = new Payment { Id = 1 };
var shipment = new Shipment { Id = 1 };

response = await restApiClient.InsertIntoAsync(order);
response = await restApiClient.InsertIntoAsync(payment);
response = await restApiClient.InsertIntoAsync(shipment);
```

Left joins can be also constructed in the following (less readable) way:

```C#
var query2 = KSqlDBContext.CreateQueryStream<Order>()
  .GroupJoin(Source.Of<Payment>(), c => c.OrderId, c => c.Id, (order, gj) => new
                                                                             {
                                                                               order,
                                                                               grouping = gj
                                                                             })
  .SelectMany(c => c.grouping.DefaultIfEmpty(), (o, s1) => new
                                                           {
                                                             o.order.OrderId,
                                                             shipmentId = s1.Id,
                                                           });
```

Equivalent KSQL:

```KSQL
SELECT order.OrderId OrderId, s1.Id AS shipmentId FROM Orders order
LEFT JOIN Payments s1
ON order.OrderId = s1.Id
EMIT CHANGES;
```

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

### Full Outer Join
**v1.0.0**

FULL OUTER joins will contain leftRecord-NULL or NULL-rightRecord records in the result stream, which means that the join contains NULL values for fields coming from a stream where no match is made.
Define nullable primitive value types in POCOs:
```C#
public record Movie
{
  public long RowTime { get; set; }
  public string Title { get; set; }
  public int? Id { get; set; }
  public int? Release_Year { get; set; }
}

public class Lead_Actor
{
  public string Title { get; set; }
  public string Actor_Name { get; set; }
}
```

```C#
var source = new KSqlDBContext(@"http:\\localhost:8088")
  .CreateQueryStream<Movie>()
  .FullOuterJoin(
    Source.Of<Lead_Actor>("Actors"),
    movie => movie.Title,
    actor => actor.Title,
    (movie, actor) => new
    {
      movie.Id,
      Title = movie.Title,
      movie.Release_Year,
      ActorTitle = actor.Title
    }
  );
```

Generated KSQL:
```KSQL
SELECT m.Id Id, m.Title Title, m.Release_Year Release_Year, l.Title ActorTitle FROM movies_test m
FULL OUTER JOIN lead_actor_test l
ON m.Title = l.Title
EMIT CHANGES;
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
