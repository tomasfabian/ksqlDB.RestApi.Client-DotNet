﻿using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  [TestClass]
  public class OperatorBetweenKSqlVisitorTests : TestBase
  {
    [TestMethod]
    public void Visit_Between()
    {
      //Arrange
      Expression<Func<Tweet, bool>> expression = t => t.Id.Between(1, 100);
      StringBuilder stringBuilder = new();

      //Act
      new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
      var ksql = stringBuilder.ToString();

      //Assert
      ksql.Should().Be("Id BETWEEN 1 AND 100");
    }

    [TestMethod]
    public void Visit_NotBetween()
    {
      //Arrange
      Expression<Func<Tweet, bool>> expression = t => t.Id.NotBetween(1, 100);
      StringBuilder stringBuilder = new();

      //Act
      new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
      var ksql = stringBuilder.ToString();

      //Assert
      ksql.Should().Be("Id NOT BETWEEN 1 AND 100");
    }
  }
}