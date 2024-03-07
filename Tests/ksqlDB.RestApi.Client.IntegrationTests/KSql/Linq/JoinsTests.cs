using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.Helpers;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class JoinsTests : Infrastructure.IntegrationTests
{
  private static MoviesProvider moviesProvider = null!;


  private static readonly EntityCreationMetadata OrderEntityCreationMetadata = new()
  {
    KafkaTopic = nameof(Order) + "-TestJoin",
    Partitions = 1
  };

  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();
      
    moviesProvider = new MoviesProvider(RestApiProvider);

    var response = await RestApiProvider.CreateStreamAsync<Order>(OrderEntityCreationMetadata, ifNotExists: true);

    await moviesProvider.DropTablesAsync();
      
    await Task.Delay(TimeSpan.FromSeconds(1));

    await moviesProvider.CreateTablesAsync();

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor1);
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    await moviesProvider.DropTablesAsync();
  }

  private static string MoviesTableName => MoviesProvider.MoviesTableName;
  private static string ActorsTableName => MoviesProvider.ActorsTableName;

  [Test]
  public async Task Join()
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = Context.CreateQueryStream<Movie>(MoviesTableName)
      .Join(
        Source.Of<Lead_Actor>(ActorsTableName),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          Title = movie.Title,
          movie.Release_Year,
          ActorTitle = actor.Title,
          Substr = K.Functions.Substring(actor.Title, 2, 4)
        }
      )
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    
    Assert.AreEqual(MoviesProvider.Movie1.Title, actualValues[0].Title);
    Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[0].Title);
    Assert.AreEqual("lien", actualValues[0].Substr);
    Assert.AreEqual(MoviesProvider.Movie1.Release_Year, actualValues[0].Release_Year);
  }

  [Test]
  public async Task LeftJoin()
  {
    //Arrange
    int expectedItemsCount = 2;

    var source = Context.CreateQueryStream<Movie>(MoviesTableName)
      .LeftJoin(
        Source.Of<Lead_Actor>(ActorsTableName),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          Title = movie.Title,
          movie.Release_Year,
          Substr = K.Functions.Substring(movie.Title, 2, 4),
          ActorTitle = actor.Title,
        }
      )
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    Assert.AreEqual(MoviesProvider.Movie1.Title, actualValues[0].Title);
    Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[0].Title);

    actualValues[1].Substr.Length.Should().Be(4);
    actualValues[1].Release_Year.Should().BeOneOf(MoviesProvider.Movie1.Release_Year, MoviesProvider.Movie2.Release_Year);
    actualValues[1].ActorTitle.Should().BeOneOf(null, MoviesProvider.Movie1.Title, MoviesProvider.Movie2.Title);
  }

  public record Movie2 : Record
  {
    public string Title { get; set; } = null!;
    public int? Id { get; set; }
    public int? Release_Year { get; set; }
  }

  [Test]
  public async Task FullOuterJoin()
  {
    await FullOuterJoinTest(Context.CreateQueryStream<Movie2>(MoviesTableName));
  }

  [Test]
  public async Task FullOuterJoin_QueryEndPoint()
  {
    await FullOuterJoinTest(Context.CreateQuery<Movie2>(MoviesTableName));
  }

  public static async Task FullOuterJoinTest(IQbservable<Movie2> querySource)
  {
    //Arrange
    int expectedItemsCount = 3;
        
    await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor2);

    var source = querySource
      .FullOuterJoin(
        Source.Of<Lead_Actor>(ActorsTableName),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          Title = movie.Title,
          movie.Release_Year,
          ActorTitle = actor.Title
        }
      )
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);
        
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    actualValues[2].Title.Should().BeOneOf(MoviesProvider.Movie1.Title, MoviesProvider.Movie2.Title, null);
  }

  private class Order
  {
    public int OrderId { get; init; }
    public int PaymentId { get; init; }
    public int ShipmentId { get; init; }
  }

  #region RightJoin

  [Test]
  public async Task RightJoin()
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = Context.CreateQueryStream<Lead_Actor>(ActorsTableName)
      .RightJoin(
        Source.Of<Movie>(MoviesTableName),
        actor => actor.Title,
        movie => movie.Title,
        ( actor, movie) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          Substr = K.Functions.Substring(movie.Title, 2, 4),
          ActorTitle = actor.Title,
        }
      )
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    Assert.AreEqual(MoviesProvider.Movie1.Title, actualValues[0].Title);
    Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[0].Title);

    actualValues[0].Substr.Length.Should().Be(4);
    actualValues[0].Release_Year.Should().BeOneOf(MoviesProvider.Movie1.Release_Year, MoviesProvider.Movie2.Release_Year);
    actualValues[0].ActorTitle.Should().BeOneOf(null, MoviesProvider.Movie1.Title, MoviesProvider.Movie2.Title);
  }

  #endregion

  #region MultipleJoins

  class Payment
  {
    [Key]
    public int Id { get; init; }
  }

  record Shipment
  {
    [Key]
    public int? Id { get; init; }
  }

  struct Foo
  {
    public int Prop { get; set; }
  }

  [Test]
  public async Task MultipleJoins_QuerySyntax()
  {
    //Arrange
    int expectedItemsCount = 1;

    var response = await RestApiProvider.CreateStreamAsync<Order>(OrderEntityCreationMetadata, ifNotExists: true);
    response = await RestApiProvider.CreateTableAsync<Payment>(OrderEntityCreationMetadata with { KafkaTopic = nameof(Payment) + "-TestJoin" }, ifNotExists: true);
    response = await RestApiProvider.CreateTableAsync<Shipment>(OrderEntityCreationMetadata with { KafkaTopic = nameof(Shipment) + "-TestJoin" }, ifNotExists: true);

    var ksqlDbUrl = TestConfig.KSqlDbUrl;

    var context = new KSqlDBContext(ksqlDbUrl);

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
      .Take(1);
      
    var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
    var payment = new Payment { Id = 1 };
    var shipment = new Shipment { Id = 1 };

    response = await RestApiProvider.InsertIntoAsync(order);
    response = await RestApiProvider.InsertIntoAsync(payment);
    response = await RestApiProvider.InsertIntoAsync(shipment);

    //Act
    var actualValues = await CollectActualValues(query.ToAsyncEnumerable(), expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    actualValues[0].orderId.Should().Be(1);
    actualValues[0].paymentId.Should().Be(1);
    var shipmentId = actualValues[0].shipmentId;
    if (shipmentId.HasValue)
      shipmentId.Should().Be(1);
  }

  [Test]
  public async Task JoinWithin_QuerySyntax()
  {
    //Arrange
    int expectedItemsCount = 1;

    var entityCreationMetadata = new EntityCreationMetadata
    {
      EntityName = nameof(Payment) + "Stream",
      KafkaTopic = nameof(Payment) + "TestJoin",
      Partitions = 1
    };

    var response = await RestApiProvider.CreateStreamAsync<Payment>(entityCreationMetadata, ifNotExists: true);

    var context = new KSqlDBContext(TestConfig.KSqlDbUrl);

    var query = from o in context.CreateQueryStream<Order>()
      join p in Source.Of<Payment>(nameof(Payment) + "Stream").Within(Duration.OfSeconds(0), Duration.OfSeconds(25)) on o.OrderId equals p.Id
      select new
      {
        orderId = o.OrderId,
        paymentId = p.Id
      };

    var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
    var payment = new Payment { Id = 1 };

    response = await RestApiProvider.InsertIntoAsync(order);
    response = await RestApiProvider.InsertIntoAsync(payment, new InsertProperties { EntityName = nameof(Payment) + "Stream" });

    //Act
    var actualValues = await CollectActualValues(query.ToAsyncEnumerable(), expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    actualValues[0].orderId.Should().Be(1);
  }

  private class Nested
  {
    public string Prop { get; init; } = null!;
  }

  private class PaymentExt
  {
    [Key]
    public int Id { get; init; }
    public Nested Nested { get; init; } = null!;
  }

  [Test]
  public async Task JoinWithNestedPropertyAccessor_QuerySyntax()
  {
    //Arrange
    int expectedItemsCount = 1;

    var response = await RestApiProvider.CreateTypeAsync<Nested>();
    var entityCreationMetadata = new EntityCreationMetadata(nameof(PaymentExt) + "-TestJoin", partitions: 1);
    response = await RestApiProvider.CreateTableAsync<PaymentExt>(entityCreationMetadata, ifNotExists: true);

    var context = new KSqlDBContext(TestConfig.KSqlDbUrl);
      
    string prop = "Nested";

    var query = from o in context.CreateQueryStream<Order>()
      join p in Source.Of<PaymentExt>() on o.OrderId equals p.Id
      where p.Nested.Prop == prop
      select new
      {
        p.Nested.Prop,
        orderId = o.OrderId,
      };

    var order = new Order { OrderId = 1, PaymentId = 1, ShipmentId = 1 };
    var payment = new PaymentExt { Id = 1, Nested = new Nested() { Prop = prop } };

    response = await RestApiProvider.InsertIntoAsync(payment);
    response = await RestApiProvider.InsertIntoAsync(order);

    //Act
    var actualValues = await CollectActualValues(query.ToAsyncEnumerable(), expectedItemsCount);

    //Assert
    actualValues[0].Prop.Should().Be(prop);
    actualValues[0].orderId.Should().Be(1);
  }

  #endregion
}
