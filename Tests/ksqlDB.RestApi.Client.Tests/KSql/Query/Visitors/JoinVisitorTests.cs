using System;
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
      var expectedQuery = @$"SELECT M.Id Id, M.Title Title, M.Release_Year Release_Year, TRIM(L.Actor_Name) ActorName, UCASE(L.Actor_Name) UpperActorName, L.Title AS ActorTitle FROM Movies M
INNER JOIN {joinItemName}s L
ON M.Title = L.Title
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
      var expectedQuery = @"SELECT M.Id Id, M.Title Title, M.Release_Year Release_Year, TRIM(L.Actor_Name) ActorName, UCASE(L.Actor_Name) UpperActorName, L.Title AS ActorTitle FROM Movies M
INNER JOIN Lead_Actors L
ON M.Title = L.Title
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
          (movie, actor) => new
          {
            Title = movie.Title,
            Length = actor.Actor_Name.Length
          }
        );

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @"SELECT M.Title Title, LEN(L.Actor_Name) Length FROM Movies M
INNER JOIN Lead_Actors L
ON M.Title = L.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    [TestMethod]
    public void SameStreamName_BuildKSql_PrintsDifferentAliases()
    {
      //Arrange
      var query = KSqlDBContext.CreateQueryStream<Movie>()
        .Join(
          Source.Of<Movie>(),
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
      var expectedQuery = @"SELECT M.Title Title FROM Movies M
INNER JOIN Movies M1
ON M.Title = M1.Title
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
          }
        );

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @"SELECT M.Title Title FROM Movies M
INNER JOIN Actors A
ON M.Title = A.Title
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
      var expectedQuery = @"SELECT M.Title Title, A.Actor_Name AS ActorName FROM Movies M
INNER JOIN Actors A
ON M.Title = A.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

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
      var expectedQuery = @"SELECT M.Id Id, M.Title Title, M.Release_Year Release_Year, TRIM(L.Actor_Name) ActorName, UCASE(L.Actor_Name) UpperActorName, L.Title AS ActorTitle FROM Movies M
LEFT JOIN Lead_Actors L
ON M.Title = L.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    [TestMethod]
    [Ignore("TODO:")]
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
      var expectedQuery = @"SELECT M.Id Id, UCASE(A.Actor_Name) UpperActorName, A.Title AS ActorTitle FROM Movies M
LEFT JOIN Actors A
ON M.Title = A.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    [TestMethod]
    [Ignore("TODO:")]
    public void GroupJoinSelectMany_BuildKSql_Prints()
    {
      var query = KSqlDBContext.CreateQueryStream<Movie>()
        .GroupJoin(Source.Of<Lead_Actor>("Actors"), c => c.Title, d => d.Title, (movie, gj) => new
        {
          movie,
          grouping = gj
        }).SelectMany(c => c.grouping.DefaultIfEmpty(), (movie2, l) => new
                                                                       {
                                                                         movie2.movie.Id,
                                                                         UpperActorName = l.Actor_Name.ToUpper(),
                                                                         ActorTitle = l.Title
                                                                       });

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @"SELECT M.Id Id, UCASE(A.Actor_Name) UpperActorName, A.Title AS ActorTitle FROM Movies M
LEFT JOIN Actors A
ON M.Title = A.Title
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
            Title = movie.Title,
          }
        );

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @"SELECT M.Title Title FROM Movies M
LEFT JOIN Actors A
ON M.Title = A.Title
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
          }
        );

      //Act
      var ksql = query.ToQueryString();

      //Assert
      var expectedQuery = @"SELECT M.Title Title FROM Movies M
FULL OUTER JOIN Actors A
ON M.Title = A.Title
 EMIT CHANGES;";

      ksql.Should().Be(expectedQuery);
    }

    #endregion
  }
}