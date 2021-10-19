using System;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.KSql.Query.Context;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Linq
{
  [TestClass]
  public class QbservableExtensionsWindowsTests : TestBase
  {
    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithWindowSession()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new SessionWindow(Duration.OfSeconds(5)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW SESSION (5 SECONDS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithHoppingWindow()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW HOPPING (SIZE 5 SECONDS, ADVANCE BY 5 SECONDS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void WindowStartAndEnd_BuildKSql_PrintsQueryWithHoppingWindow()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)))
        .Select(g => new { g.WindowStart, g.WindowEnd, CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT WindowStart, WindowEnd, CardNumber, COUNT(*) Count FROM Transactions WINDOW HOPPING (SIZE 5 SECONDS, ADVANCE BY 5 SECONDS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithHoppingWindowAdvanceBy()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new HoppingWindows(Duration.OfMinutes(5)).WithAdvanceBy(Duration.OfMinutes(4)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW HOPPING (SIZE 5 MINUTES, ADVANCE BY 4 MINUTES) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithHoppingWindowRetention()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)).WithRetention(Duration.OfDays(7)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW HOPPING (SIZE 5 SECONDS, ADVANCE BY 5 SECONDS, RETENTION 7 DAYS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AdvanceByIsBiggerThenWindowSize_BuildKSql_Throws()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new HoppingWindows(Duration.OfSeconds(5)).WithAdvanceBy(Duration.OfSeconds(7)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act

      //Assert
    }

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithTumblingWindow()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new TimeWindows(Duration.OfSeconds(5)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW TUMBLING (SIZE 5 SECONDS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQueryWithTumblingWindowAndGracePeriod()
    {
      //Arrange
      var context = new TransactionsDbProvider(TestParameters.KsqlDBUrl);

      var grouping = context.CreateQueryStream<Transaction>()
        .GroupBy(c => c.CardNumber)
        .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM Transactions WINDOW TUMBLING (SIZE 5 SECONDS, GRACE PERIOD 2 HOURS) GROUP BY CardNumber EMIT CHANGES;");
    }

    [TestMethod]
    public void QueriesFromSameCreateStreamSetShouldNotAffectEachOther()
    {
      //Arrange
      var options = new KSqlDBContextOptions(TestParameters.KsqlDBUrl)
      {
        ShouldPluralizeFromItemName = false
      };

      var context = new TransactionsDbProvider(options);

      var grouping1 = context.CreateQueryStream<Transaction>("authorization_attempts_1")
        .GroupBy(c => c.CardNumber)
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      var grouping2 = context.CreateQueryStream<Transaction>("authorization_attempts_2")
        .GroupBy(c => c.CardNumber)
        .Select(g => new { CardNumber = g.Key, Count = g.Count() });

      var take = grouping2.Take(2);

      //Act
      var ksql1 = grouping1.ToQueryString();
      var ksql2 = grouping2.ToQueryString();
      var ksqlTake = take.ToQueryString();

      //Assert
      ksql1.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM authorization_attempts_1 GROUP BY CardNumber EMIT CHANGES;");
      ksql2.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM authorization_attempts_2 GROUP BY CardNumber EMIT CHANGES;");
      ksqlTake.Should().BeEquivalentTo("SELECT CardNumber, COUNT(*) Count FROM authorization_attempts_2 GROUP BY CardNumber EMIT CHANGES LIMIT 2;");
    }

    class TransactionsDbProvider : TestableDbProvider<Transaction>
    {
      public TransactionsDbProvider(string ksqlDbUrl) : base(ksqlDbUrl)
      {
        RegisterKSqlQueryGenerator = false;
      }

      public TransactionsDbProvider(KSqlDBContextOptions contextOptions) : base(contextOptions)
      {
        RegisterKSqlQueryGenerator = false;
      }
    }
  }
}