using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.Http;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class ComplexTypesTests
{
  private IKSqlDbRestApiClient restApiClient = null!;
  protected KSqlDBContext Context = null!;

  [SetUp]
  public void Initialize()
  {
    var ksqlDbUrl = @"http://localhost:8088";

    var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

    restApiClient = new KSqlDbRestApiClient(httpClientFactory);

    var contextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = true
    };

    Context = new KSqlDBContext(contextOptions);
  }

  [Test]
  public async Task ReceiveArrayWithComplexElements()
  {
    //Arrange
    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
Drop type EventCategory;
Drop table Events;
"));

    //Act
    httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
    httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata() { KafkaTopic = "Events", Partitions = 1 });

    var eventCategory = new EventCategory()
    {
      Name = "xyz"
    };

    var testEvent = new Event
    {
      Id = 1,
      Places = new[] { "Place1", "Place2" },
      Categories = new[] { eventCategory, new EventCategory { Name = "puk" } }
    };

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    var receivedValues = new List<Event>();
    var subscription = Context.CreateQueryStream<Event>().Take(1)
      .Subscribe(value =>
        {
          receivedValues.Add(value);
        }, error =>
        {
          semaphoreSlim.Release();
        },
        () =>
        {
          semaphoreSlim.Release();
        });

    httpResponseMessage = await restApiClient.InsertIntoAsync(testEvent);

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

    //Assert
    await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10));

    receivedValues.Count.Should().Be(1);
    receivedValues[0].Places.Length.Should().Be(2);
    receivedValues[0].Categories.ToArray()[0].Name.Should().Be("xyz");
    receivedValues[0].Categories.ToArray()[1].Name.Should().Be("puk");

    using (subscription) { }
  }

  class Foo : Dictionary<string, int>
  {
  }

  [Test]
  public void TransformMap_WithNestedMap()
  {
    //Arrange
    var value = new Dictionary<string, IDictionary<string, int>>()
      { { "a", new Dictionary<string, int>() { { "a", 1 } } } };

    string ksql =
      Context.CreateQueryStream<object>(fromItemName: "Events")
        .Select(_ => new
        {
          Dict = K.Functions.Transform(value, (k, v) => k.ToUpper(), (k, v) => v["a"] + 1)
        })
        .ToQueryString();

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    //Act
    var source = Context.CreateQueryStream<Foo>(queryStreamParameters)
      .Take(1)
      .ToEnumerable()
      .First();

    //Assert
    source["A"].Should().Be(2);
  }

  record MyType
  {
    public int a { get; set; }
    public int b { get; set; }
  }

  [Test]
  public void TransformMap_WithNestedStruct()
  {
    //Arrange
    var value = new Dictionary<string, MyType>
      { { "a", new MyType { a = 1, b = 2 } } };

    string ksql =
      Context.CreateQueryStream<object>(fromItemName: "Events")
        .Select(_ => new
        {
          Dict = K.Functions.Transform(value, (k, v) => k.ToUpper(), (k, v) => v.a + 1)
        })
        .ToQueryString();

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    //Act
    var source = Context.CreateQueryStream<Foo>(queryStreamParameters)
      .Take(1)
      .ToEnumerable()
      .First();

    //Assert
    source["A"].Should().Be(2);
  }

  record Event
  {
    [Key]
    public int Id { get; set; }

    public string[] Places { get; init; } = null!;
    public IEnumerable<EventCategory> Categories { get; init; } = null!;
  }

  record EventCategory
  {
    public string Name { get; init; } = null!;
  }
}
