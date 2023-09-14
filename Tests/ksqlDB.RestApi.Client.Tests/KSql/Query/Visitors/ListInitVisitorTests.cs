using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  public class ListInitVisitorTests
  {
      private StringBuilder stringBuilder = null!;
      private KSqlQueryMetadata queryMetadata = null!;
      private ListInitVisitor listInitVisitor = null!;

      [SetUp]
      public void Setup()
      {
        stringBuilder = new StringBuilder();
        queryMetadata = new KSqlQueryMetadata();
        listInitVisitor = new ListInitVisitor(stringBuilder, queryMetadata);
      }

      [Test]
      public void VisitList()
      {
        //Arrange
        var list = new List<int> { 1, 2 };
        Expression expression = Expression.Constant(list);

        //Act
        listInitVisitor.Visit(expression);

        //Assert
        var ksql = stringBuilder.ToString();
        ksql.Should().Be("ARRAY[1, 2]");
      }

      [Test]
      public void VisitDictionary()
      {
        //Arrange
        var dictionary = new Dictionary<string, int>
        {
          {"one", 1},
          {"two", 2}
        };
        Expression expression = Expression.Constant(dictionary);

        //Act
        listInitVisitor.Visit(expression);

        //Assert
        var ksql = stringBuilder.ToString();
        ksql.Should().Be("MAP('one' := 1, 'two' := 2)");
      }
    }
}
