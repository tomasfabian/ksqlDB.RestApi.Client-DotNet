using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  [TestClass]
  public class JoinVisitorTests : TestBase
  {
    private KSqlDBContext KSqlDBContext { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      KSqlDBContext = new KSqlDBContext(contextOptions);
    }

    private string MovieAlias => "movie";
    private string ActorAlias => "actor";

    #region Join

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void SameStreamName_BuildKSql_PrintsDifferentAliases()
    {
      //Arrange
      var query = KSqlDBContext.CreateQueryStream<Movie>()
        .Join(
          Source.Of<MovieExt>(),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            Title = movie.Title,
          }
        );

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN MovieExts M1
ON {MovieAlias}.Title = M1.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    [TestMethod]
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
INNER JOIN Actors A
ON {MovieAlias}.Title = A.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    [TestMethod]
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
    
    [TestMethod]
    public void InnerJoinQuerySyntax_BuildKSql_Prints()
    {
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

    [TestMethod]
    public void MultipleInnerJoinsQuerySyntax_BuildKSql_Prints()
    {
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
      public IDictionary<string, City> Dictionary { get; set; }
    }
    private class City
    {
      public int[] Values { get; set; }
    }

    [TestMethod]
    [Ignore("TODO")]
    public void JoinWithInvocationFunction_BuildKSql_Prints()
    {
      var query = from o in KSqlDBContext.CreateQueryStream<Order>()
        join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
        join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
        select new
               {
                 A = K.Functions.Transform(lm.Dictionary, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x)),
                 //orderId = o.OrderId,
                 //shipmentId = s1.Id
               };

      //Act
      var ksql = query.ToQueryString();

      //Assert
      string lambdaAlias = "lm";

      //var expectedQuery = @$"SELECT TRANSFORM({lambdaAlias}.{nameof(LambdaMap.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x)) A, O.OrderId AS orderId, S1.Id AS shipmentId FROM Orders O
      var expectedQuery = @$"SELECT TRANSFORM({lambdaAlias}.{nameof(LambdaMap.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x)) A FROM Orders O
INNER JOIN LambdaMaps {lambdaAlias}
ON O.OrderId = {lambdaAlias}.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
 EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedQuery);
    }

    [TestMethod]
    public void MultipleInnerJoinsQuerySyntax_WithTake_BuildKSql_Prints()
    {
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

    [TestMethod]
    public void JoinAndLeftJoin_WithTake_BuildKSql_Prints()
    {
      var query = from o in KSqlDBContext.CreateQueryStream<Order>()
        join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
        join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id into gj
        from sa in gj.DefaultIfEmpty()
        select new
               {
                 orderId = o.OrderId,
                 shipmentId = sa.Id,
                 paymentId = p1.Id,
               };

      //Act
      var ksql = query.ToQueryString();

      //Assert
      string shipmentsAlias = "sa";

      var expectedQuery = @$"SELECT O.OrderId AS orderId, {shipmentsAlias}.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
LEFT JOIN Shipments {shipmentsAlias}
ON O.OrderId = {shipmentsAlias}.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
 EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedQuery);
    }

    //TODO:
    //SELECT
    //o.id as orderId,
    //o.itemid as itemId,
    //s.id as shipmentId,
    //p.id as paymentId
    //FROM orders o
    //INNER JOIN payments p WITHIN 1 HOURS ON p.id = o.id
    //INNER JOIN shipments s WITHIN 2 HOURS ON s.id = o.id
    // EMIT CHANGES;

    #endregion

    #region LeftJoin

    [TestMethod]
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

    [TestMethod]
    public void LeftJoinQuerySyntax_BuildKSql_Prints()
    {
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

    [TestMethod]
    public void GroupJoinSelectMany_BuildKSql_Prints()
    {
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
      public string ItemName { get; set; }
    }

    [TestMethod]
    public void MultipleLeftJoinsQuerySyntax_BuildKSql_Prints()
    {
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

    [TestMethod]
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

    [TestMethod]
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
  }
}