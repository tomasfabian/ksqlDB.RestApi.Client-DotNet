using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using NUnit.Framework;
using UnitTests;
using TestParameters = ksqlDB.Api.Client.Tests.Helpers.TestParameters;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

public class JoinVisitorTests : TestBase
{
  private KSqlDBContext KSqlDBContext { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
    KSqlDBContext = new KSqlDBContext(contextOptions);
  }

  private string MovieAlias => "movie";
  private string ActorAlias => "actor";

  #region Join

  [Test]
  public void Join_BuildKSql_PrintsInnerJoin()
  {
    //Arrange
    var joinItemName = "LeadActor";

    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<Lead_Actor>(joinItemName),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN {joinItemName}s {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void Join_BuildKSql_PrintsInnerJoin_PluralizedJoinItem()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN Lead_Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void JoinAndSelectWithAliases_BuildKSql_PrintsInnerJoin()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (myMovie, actor) => new
        {
          Title = myMovie.Title,
          Length = actor.Actor_Name.Length
        }
      );

    string myMovieAlias = "myMovie";

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {myMovieAlias}.Title Title, LEN({ActorAlias}.Actor_Name) Length FROM Movies {myMovieAlias}
INNER JOIN Lead_Actors {ActorAlias}
ON {myMovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  private class MovieExt : Movie
  {
  }

  [Test]
  public void SameStreamName_BuildKSql_PrintsDifferentAliases()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<MovieExt>(),
        movie => movie.Title,
        ext => ext.Title,
        (movie, ext) => new
        {
          Title = movie.Title,
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN MovieExts ext
ON {MovieAlias}.Title = ext.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void InnerJoinOverrideStreamName_NoProjectionFromJoinTable_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void InnerJoinOverrideStreamName_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .Join(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }
    
  [Test]
  public void InnerJoinQuerySyntax_BuildKSql_Prints()
  {
    //Arrange
    var query = from movie in KSqlDBContext.CreateQueryStream<Movie>()
      join actor in Source.Of<Lead_Actor>("Actors") on movie.Title equals actor.Title
      select new
      {
        movie.Title,
        ActorName = actor.Actor_Name
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  private class Payment
  {
    public int Id { get; set; }
  }
  private record Shipment
  {
    public int Id { get; set; }
  }
  struct Foo
  {
    public int Prop { get; set; }
  }

  [Test]
  public void MultipleInnerJoinsQuerySyntax_BuildKSql_Prints()
  {
    //Arrange
    var value = new Foo { Prop = 42 };

    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        value,
        orderId = o.OrderId,
        shipmentId = s1.Id,
        paymentId = p1.Id,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @"SELECT STRUCT(Prop := 42) value, O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  class LambdaMap
  {
    public int Id { get; set; }
    public IDictionary<string, City> Dictionary { get; set; } = null!;
    public Nested Nested { get; set; } = null!;
  }
  private class City
  {
    public int[] Values { get; set; } = null!;
  }

  private class Nested
  {
    public string Prop { get; set; } = null!;
  }

  [Test]
  public void JoinWithInvocationFunction_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        A = K.Functions.Transform(lm.Dictionary, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x)),
        orderId = o.OrderId,
        shipmentId = s1.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string lambdaAlias = "lm";
    string shipmentsAlias = "s1";

    var expectedQuery = @$"SELECT TRANSFORM({lambdaAlias}.{nameof(LambdaMap.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x)) A, O.OrderId AS orderId, {shipmentsAlias}.Id AS shipmentId FROM Orders O
INNER JOIN Shipments {shipmentsAlias}
ON O.OrderId = {shipmentsAlias}.Id
INNER JOIN LambdaMaps {lambdaAlias}
ON O.OrderId = {lambdaAlias}.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("TODO:")]
  public void JoinWithSeveralOnConditions_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join lm in Source.Of<LambdaMap>() on new { OrderId = o.OrderId, NestedProp = "Nested" } equals new { OrderId = lm.Id, NestedProp = lm.Nested.Prop }
      select new
      {
        lm.Nested.Prop,
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    //TODO:
  }

  [Test]
  public void JoinWithNestedType_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      where lm.Nested.Prop == "Nested"
      select new
      {
        lm.Nested.Prop,
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string lambdaAlias = "lm";

    var expectedQuery = @$"SELECT {lambdaAlias}.Nested->Prop Prop, O.OrderId AS orderId FROM Orders O
INNER JOIN LambdaMaps {lambdaAlias}
ON O.OrderId = {lambdaAlias}.Id
WHERE {lambdaAlias}.Nested->Prop = 'Nested' EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void JoinWithFunctionAndNestedType_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      select new
      {
        Concat = K.Functions.Concat(lm.Nested.Prop, "_new"),
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string ordersAlias = "o";
    string lambdaAlias = "lm";

    var expectedQuery = @$"SELECT CONCAT({lambdaAlias}.Nested->Prop, '_new') Concat, {ordersAlias}.OrderId AS orderId FROM Orders {ordersAlias}
INNER JOIN LambdaMaps {lambdaAlias}
ON {ordersAlias}.OrderId = {lambdaAlias}.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void MultipleInnerJoinsQuerySyntax_WithTake_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        orderId = o.OrderId,
        shipmentId = s1.Id,
        paymentId = p1.Id,
      };

    query = query.Take(2);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @"SELECT O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES LIMIT 2;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void JoinAndLeftJoin_WithTake_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id into gj
      from sa in gj.DefaultIfEmpty()
      select new
      {
        orderId = o.OrderId,
        shipmentId = sa.Id,
        paymentId = p1.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string shipmentsAlias = "sa";

    var expectedQuery = @$"SELECT o.OrderId AS orderId, {shipmentsAlias}.Id AS shipmentId, p1.Id AS paymentId FROM Orders o
LEFT JOIN Shipments {shipmentsAlias}
ON o.OrderId = {shipmentsAlias}.Id
INNER JOIN Payments p1
ON o.OrderId = p1.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void JoinWithinTimeUnit_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join p in Source.Of<Payment>().Within(Duration.OfHours(1)) on o.OrderId equals p.Id
      join s in Source.Of<Shipment>().Within(Duration.OfDays(5)) on o.OrderId equals s.Id
      select new
      {
        orderId = o.OrderId,
        shipmentId = s.Id,
        paymentId = p.Id
      };
      
    //Act
    var ksql = query.ToQueryString();

    //Assert
    string ordersAlias = "o";

    var expectedQuery = @$"SELECT {ordersAlias}.OrderId AS orderId, s.Id AS shipmentId, p.Id AS paymentId FROM Orders {ordersAlias}
INNER JOIN Shipments s
WITHIN 5 DAYS ON {ordersAlias}.OrderId = s.Id
INNER JOIN Payments p
WITHIN 1 HOURS ON {ordersAlias}.OrderId = p.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void JoinWithinTimeUnit_BeforeAfter_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDBContext.CreateQueryStream<Order>()
      join p in Source.Of<Payment>().Within(Duration.OfHours(1), Duration.OfDays(5)) on o.OrderId equals p.Id
      select new
      {
        orderId = o.OrderId,
        paymentId = p.Id
      };
      
    //Act
    var ksql = query.ToQueryString();

    //Assert
    string ordersAlias = "o";

    var expectedQuery = @$"SELECT {ordersAlias}.OrderId AS orderId, p.Id AS paymentId FROM Orders {ordersAlias}
INNER JOIN Payments p
WITHIN (1 HOURS, 5 DAYS) ON {ordersAlias}.OrderId = p.Id
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  public void Join_KSqlFunctionKeySelector_ShouldBeWithoutAlias()
  {
    //Arrange
    var query = KSqlDBContext
      .CreateQueryStream<Movie>("movies")
      .Join(Source.Of<Lead_Actor>("actors").Within(Duration.OfDays(1)),
        movie => K.Functions.ExtractJsonField(movie!.Title, "$.movie_title"),
        actor => K.Functions.ExtractJsonField(actor!.Actor_Name, "$.actor_name"),
        (movie, actor) => new { EnduserId = actor.Title, Name = actor.Actor_Name, Raw = movie.Title });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @"SELECT actor.Title AS EnduserId, actor.Actor_Name AS Name, movie.Title AS Raw FROM movies movie
INNER JOIN actors actor
WITHIN 1 DAYS ON EXTRACTJSONFIELD(movie.Title, '$.movie_title') = EXTRACTJSONFIELD(actor.Actor_Name, '$.actor_name')
EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedQuery);
  }
  #endregion

  #region LeftJoin

  [Test]
  public void LeftJoin_BuildKSql_PrintsLeftJoin()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .LeftJoin(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT movie.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Lead_Actors {ActorAlias}
ON movie.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void LeftJoinQuerySyntax_BuildKSql_Prints()
  {
    //Arrange
    var query = 
      from movie in KSqlDBContext.CreateQueryStream<Movie>()
      join actor in Source.Of<Lead_Actor>("Actors") 
        on movie.Title equals actor.Title into gj
      from a in gj.DefaultIfEmpty()
      select new
      {
        movie.Id,
        UpperActorName = a.Actor_Name.ToUpper(),
        ActorTitle = a.Title
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void GroupJoinSelectMany_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .GroupJoin(Source.Of<Lead_Actor>("Actors"), c => c.Title, d => d.Title, (movie, gj) => new
      {
        movie,
        grouping = gj
      }).SelectMany(c => c.grouping.DefaultIfEmpty(), (movie, a) => new
      {
        movie.movie.Id,
        UpperActorName = a.Actor_Name.ToUpper(),
        ActorTitle = a.Title
      });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  private class Order
  {
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ItemId { get; set; }
  }
  private sealed class Customer
  {
    public int CustomerId { get; set; }
  }
  private sealed class Item
  {
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
  }

  [Test]
  public void MultipleLeftJoinsQuerySyntax_BuildKSql_Prints()
  {
    //Arrange
    var query = 
      from orders in KSqlDBContext.CreateQueryStream<Order>()
      join customer in Source.Of<Customer>() 
        on orders.CustomerId equals customer.CustomerId into gj
      from customers in gj.DefaultIfEmpty()
      join item in Source.Of<Item>() 
        on orders.ItemId equals item.ItemId into igj
      from items in igj.DefaultIfEmpty()
      select new
      {
        customerid = customers.CustomerId,
        orders.OrderId,
        items.ItemId,
        items.ItemName,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @"SELECT customers.CustomerId AS customerid, orders.OrderId OrderId, items.ItemId ItemId, items.ItemName ItemName FROM Orders orders
LEFT JOIN Items items
ON orders.ItemId = items.ItemId
LEFT JOIN Customers customers
ON orders.CustomerId = customers.CustomerId
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  [Test]
  public void LeftJoinOverrideStreamName_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .LeftJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          actor.RowTime,
          Title = movie.Title,
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {ActorAlias}.RowTime RowTime, {MovieAlias}.Title Title FROM Movies {MovieAlias}
LEFT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  #endregion

  #region FullOuterJoin

  [Test]
  public void FullOuterJoinOverrideStreamName_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .FullOuterJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
FULL OUTER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  #endregion

  #region RightJoin

  [Test]
  public void RightJoinOverrideStreamName_BuildKSql_Prints()
  {
    //Arrange
    var query = KSqlDBContext.CreateQueryStream<Movie>()
      .RightJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedQuery = @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
RIGHT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;";

    ksql.Should().Be(expectedQuery);
  }

  #endregion
}
