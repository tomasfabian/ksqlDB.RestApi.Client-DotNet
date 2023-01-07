using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using Joins.Model.Movies;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDb.RestApi.Client.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Joins.Model.Orders;

const string ksqlDbUrl = @"http:\\localhost:8088";

await using var context = new KSqlDBContext(ksqlDbUrl);

var servicesCollection = new ServiceCollection();
servicesCollection.ConfigureKSqlDb(ksqlDbUrl);

var serviceProvider = servicesCollection.BuildServiceProvider();
IKSqlDbRestApiClient ksqlDbRestApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

await SubscribeAsync(ksqlDbRestApiClient);


const string postfix = "-Join";

async Task SubscribeAsync(IKSqlDbRestApiClient restApiClient)
{
  var entityCreationMetadata = new EntityCreationMetadata
  {
    KafkaTopic = nameof(Order) + postfix,
    Partitions = 1
  };

  var response = await restApiClient.CreateStreamAsync<Order>(entityCreationMetadata, ifNotExists: true);
  var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
  Console.WriteLine(responseContent);

  response = await restApiClient.CreateTableAsync<Payment>(entityCreationMetadata with { KafkaTopic = $"{nameof(Payment)}-Join" }, ifNotExists: true);
  response = await restApiClient.CreateTableAsync<Shipment>(entityCreationMetadata with { KafkaTopic = $"{nameof(Shipment)}-Join" }, ifNotExists: true);

  var value = new Foo { Prop = 42 };

  var query = (from o in context.CreateQueryStream<Order>()
               join p1 in Source.Of<Payment>() on o.PaymentId equals p1.Id
               join s1 in Source.Of<Shipment>() on o.ShipmentId equals s1.Id into gj
               from sa in gj.DefaultIfEmpty()
               select new
               {
                 value,
                 orderId = o.OrderId,
                 shipmentId = sa.Id,
                 paymentId = p1.Id,
               })
    .Take(5);

  string ksql = query.ToQueryString();
  Console.WriteLine($"Generated ksql: {ksql}");

  using var subscription = query
    .Subscribe(c =>
    {
      Console.WriteLine($"{nameof(Order.OrderId)}: {c.orderId}");

      Console.WriteLine($"{nameof(Order.PaymentId)}: {c.paymentId}");

      if (c.shipmentId.HasValue)
        Console.WriteLine($"{nameof(Order.ShipmentId)}: {c.shipmentId}");

    }, error =>
    {
      Console.WriteLine(error.Message);
    });

  var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
  var payment = new Payment { Id = 1 };
  var shipment = new Shipment { Id = 1 };

  response = await restApiClient.InsertIntoAsync(order);
  responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

  response = await restApiClient.InsertIntoAsync(payment);

  await Task.Delay(TimeSpan.FromMilliseconds(250));
  response = await restApiClient.InsertIntoAsync(shipment);

  Console.WriteLine("Press any key to stop the subscription");

  Console.ReadKey();
}

#pragma warning disable CS8321 // Local function is declared but never used

static IDisposable JoinTables(KSqlDBContext context)
{
  var queryWithin = Source.Of<Lead_Actor>().Within(Duration.OfHours(1), Duration.OfDays(5));

  var rightJoinQueryString = context.CreateQueryStream<Movie>()
    .RightJoin(
      Source.Of<Lead_Actor>(nameof(Lead_Actor)),
      movie => movie.Title,
      actor => actor.Title,
      (movie, actor) => new
      {
        movie.Id,
        actor.Title,
      }
    ).ToQueryString();

  var query = context.CreateQueryStream<Movie>()
    .Join(
      Source.Of<Lead_Actor>(nameof(Lead_Actor)),
      movie => movie.Title,
      actor => actor.Title,
      (movie, actor) => new
      {
        movie.Id,
        Title = movie.Title,
        movie.Release_Year,
        ActorName = K.Functions.RPad(K.Functions.LPad(actor.Actor_Name.ToUpper(), 15, "*"), 25, "^"),
        ActorTitle = actor.Title,
        Substr = K.Functions.Substring(actor.Title, 2, 4)
      }
    );

  var joinQueryString = query.ToQueryString();

  return query
    .Subscribe(c => { Console.WriteLine($"{c.Id}: {c.ActorName} - {c.Title} - {c.ActorTitle}"); }, exception => { Console.WriteLine(exception.Message); });
}

static IQbservable<dynamic> LeftJoin()
{
  var query = new KSqlDBContext(@"http:\\localhost:8088").CreateQueryStream<Movie>()
    .LeftJoin(
      Source.Of<Lead_Actor>(),
      movie => movie.Title,
      actor => actor.Title,
      (movie, actor) => new
      {
        movie.Id,
        ActorTitle = actor.Title
      }
    );

  return query;
}

static IDisposable FullOuterJoinTables(KSqlDBContext context)
{
  var query = context.CreateQueryStream<MovieNullableFields>("Movies")
    .FullOuterJoin(
      Source.Of<Lead_Actor>(nameof(Lead_Actor)),
      movie => movie.Title,
      actor => actor.Title,
      (movie, actor) => new
      {
        movie.Id,
        Title = movie.Title,
        movie.Release_Year,
        ActorTitle = actor.Title,
        ActorName = actor.Actor_Name
      }
    );

  var joinQueryString = query.ToQueryString();

  return query
    .Subscribe(c =>
    {
      if (c.Id.HasValue)
        Console.WriteLine($"{c.Id}: {c.ActorName} - {c.Title} - {c.ActorTitle}");
      else
        Console.WriteLine($"No movie id: {c.ActorName} - {c.Title} - {c.ActorTitle}");
    }, exception => { Console.WriteLine(exception.Message); });
}

#pragma warning restore CS8321 // Local function is declared but never used
