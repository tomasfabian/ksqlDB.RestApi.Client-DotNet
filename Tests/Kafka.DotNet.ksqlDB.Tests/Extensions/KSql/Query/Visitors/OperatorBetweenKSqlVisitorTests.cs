using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Operators;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Visitors
{
  [TestClass]
  public class OperatorBetweenKSqlVisitorTests : TestBase
  {
    [TestMethod]
    public void Visit()
    {
      //Arrange
      Expression<Func<Tweet, bool>> expression = t => t.Id.Between(1, 100);
      StringBuilder stringBuilder = new();

      //Act
      new OperatorBetweenKSqlVisitor(stringBuilder).Visit(expression);
      var ksql = stringBuilder.ToString();

      //Assert
      ksql.Should().Be("Id BETWEEN 1 AND 100");
    }
  }
}