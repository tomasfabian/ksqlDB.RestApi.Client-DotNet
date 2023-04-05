using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;
using DataTypes.Model;
using DataTypes.Model.Events;
using DataTypes.Model.Movies;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Operators;

const string ksqlDbUrl = @"http://localhost:8088";

var servicesCollection = new ServiceCollection();
servicesCollection.ConfigureKSqlDb(ksqlDbUrl);

var serviceProvider = servicesCollection.BuildServiceProvider();
IKSqlDbRestApiClient ksqlDbRestApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();


Console.WriteLine("Press any key to stop the subscription");

Console.ReadKey();

#pragma warning disable CS8321 // Local function is declared but never used

static async Task StructType(KSqlDBContext context)
{
  var moviesStream = context.CreateQueryStream<Movie>();

  var source = moviesStream.Select(c => new
  {
    Str = new MovieStruct { Title = c.Title, Id = c.Id },
    c.Release_Year
  }).ToAsyncEnumerable();

  await foreach (var movie in source)
  {
    Console.WriteLine($"{movie.Str.Title} - {movie.Release_Year}");
  }
}

static IDisposable Arrays(KSqlDBContext context)
{
  var subscription =
    context.CreateQueryStream<Movie>()
      .Select(_ => new { FirstItem = new[] { 1, 2, 3 }[1] })
      .Subscribe(onNext: c => { Console.WriteLine($"Array first value: {c}"); },
        onError: error => { Console.WriteLine($"Exception: {error.Message}"); });

  var arrayLengthQuery = context.CreateQueryStream<Movie>()
    .Select(_ => new[] { 1, 2, 3 }.Length)
    .ToQueryString();

  return subscription;
}

static IDisposable NestedTypes(KSqlDBContext context)
{
  var disposable =
    context.CreateQueryStream<Movie>()
      .Select(c => new
      {
        MapValue = new Dictionary<string, Dictionary<string, int>>
        {
          { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
          { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
        }["a"]
      })
      .Subscribe(
        message =>
        {
          Console.WriteLine($"Dictionary with {message.MapValue.Count} values");
          foreach (var value in message.MapValue)
          {
            Console.WriteLine($"    {value.Key} - {value.Value}");
          }
        },
        error => Console.WriteLine($"Exception: {error.Message}"));

  return disposable;
}

static async Task DeeplyNestedTypes(KSqlDBContext context)
{
  var moviesStream = context.CreateQueryStream<Movie>();

  var source = moviesStream.Select(c => new
  {
    c.Id,
    Arr = new[]
    {
      new MovieStruct
      {
        Title = c.Title,
        Id = c.Id,
      },
      new MovieStruct
      {
        Title = "test",
        Id = 2,
      }
    },
    MapValue = new Dictionary<string, Dictionary<string, int>>
    {
      { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
      { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
    },
    MapArr = new Dictionary<int, string[]>
    {
      { 1, new[] { "a", "b"} },
      { 2, new[] { "c", "d"} }
    },
    Str = new MovieStruct { Title = c.Title, Id = c.Id },
    c.Release_Year
  }).ToAsyncEnumerable();

  await foreach (var movie in source)
  {
    Console.WriteLine($"{movie.Str.Title} - {movie.Release_Year}");
  }
}

#region TimeTypes

static async Task TimeTypes(IKSqlDbRestApiClient restApiClient, IKSqlDBContext context)
{
  EntityCreationMetadata metadata = new EntityCreationMetadata
  {
    KafkaTopic = nameof(Dates),
    Partitions = 1,
    Replicas = 1,
    ValueFormat = SerializationFormats.Json
  };

  var httpResponseMessage = await restApiClient.CreateStreamAsync<Dates>(metadata);

  var from = new TimeSpan(1, 0, 0);
  var to = new TimeSpan(22, 0, 0);

  var query = context.CreateQueryStream<Dates>()
    .Select(c => new { c.Ts, to, FromTime = from, DateTime.Now, New = new TimeSpan(1, 0, 0) })
    .ToQueryString();

  //.Select(c => new { c.Ts, to, FromTime = from, DateTime.Now, New = new TimeSpan(1, 0, 0) })
  using var subscription = context.CreateQueryStream<Dates>()
    .Where(c => c.Ts.Between(from, to))
    .Subscribe(onNext: m =>
    {
      Console.WriteLine($"{nameof(Dates)}: {m.Dt} : {m.Ts} : {m.DtOffset}");

      Console.WriteLine();
    }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));

  var value = new Dates
  {
    Dt = new DateTime(2021, 4, 1),
    Ts = new TimeSpan(1, 2, 3),
    DtOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4))
  };

  httpResponseMessage = await restApiClient.InsertIntoAsync(value);
  var statementResponses = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);
}

#endregion

static void Bytes(IKSqlDBContext ksqlDbContext)
{
  var ksql = ksqlDbContext.CreateQuery<Thumbnail>()
    .Select(c => new { Col = K.Functions.FromBytes(c.Image, "hex") })
    .ToQueryString();
}

static async Task SubscriptionToAComplexTypeAsync(IKSqlDbRestApiClient restApiClient, IKSqlDBContext ksqlDbContext)
{
  string typeName = nameof(EventCategory);
  var httpResponseMessage = await restApiClient.DropTypeIfExistsAsync(typeName);

  httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop table {nameof(Event)};
"));

  httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
  httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata { KafkaTopic = "Events", Partitions = 1 });

  var subscription = ksqlDbContext.CreateQueryStream<Event>()
    .Subscribe(value =>
    {
      Console.WriteLine("Categories: ");

      foreach (var category in value.Categories ?? Enumerable.Empty<EventCategory>())
      {
        Console.WriteLine($"{category.Name}");
      }
    }, error =>
    {
      Console.WriteLine(error.Message);
    });

  httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
INSERT INTO Events (Id, Places, Categories) VALUES (1, ARRAY['Place1','Place2','Place3'], ARRAY[STRUCT(Name := 'Planet Earth'), STRUCT(Name := 'Discovery')]);"));
}

#pragma warning restore CS8321 // Local function is declared but never used
