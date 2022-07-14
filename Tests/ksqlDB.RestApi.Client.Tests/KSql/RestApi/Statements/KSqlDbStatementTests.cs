using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using NUnit.Framework;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Statements;

public class KSqlDbStatementTests : TestBase
{
  static readonly string Statement = "CREATE OR REPLACE TABLE movies";

  [Test]
  public void ContentEncoding_DefaultIsUTF8()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(Statement);

    //Act
    var defaultEncoding = ksqlDbStatement.ContentEncoding;

    //Assert
    defaultEncoding.Should().Be(Encoding.UTF8);
  }

  [Test]
  public void EndpointType_DefaultIsKSql()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(Statement);

    //Act
    var endpointType = ksqlDbStatement.EndpointType;

    //Assert
    endpointType.Should().Be(EndpointType.KSql);
  }

  [Test]
  public void StatementText_WasSet()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(Statement);

    //Act
    var statementText = ksqlDbStatement.Sql;

    //Assert
    statementText.Should().Be(Statement);
  }
}