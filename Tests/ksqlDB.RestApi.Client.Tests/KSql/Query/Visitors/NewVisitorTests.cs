using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors
{
  public class NewVisitorTests
  {
    private StringBuilder stringBuilder = null!;
    private KSqlQueryMetadata queryMetadata = null!;
    private NewVisitor newVisitor = null!;

    [SetUp]
    public void Setup()
    {
      stringBuilder = new StringBuilder();
      queryMetadata = new KSqlQueryMetadata();
      newVisitor = new NewVisitor(stringBuilder, queryMetadata);
    }

    [Test]
    public void PropertyWithAliasAndCountAggregateFunction_VisitNew()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Key = l.Key, Agg = l.Count() };

      //Act
      newVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be("Key, COUNT(*) Agg");
    }

    [Test]
    public void CountWithLambda_VisitNew()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { l.Key, Agg = l.Count(x => x.Longitude) };

      //Act
      newVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo($"Key, Count({nameof(Location.Longitude)}) Agg");
    }

    [Test]
    public void Constructor()
    {
      //Arrange
      Expression<Func<object>> expression = () => new { Loc = new Location { Latitude = "1" } };

      //Act
      newVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo("STRUCT(Latitude := '1') Loc");
    }
  }
}
