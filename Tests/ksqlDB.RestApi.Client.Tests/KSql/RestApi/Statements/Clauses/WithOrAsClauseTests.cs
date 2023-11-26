using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements.Clauses;

public class WithOrAsClauseTests
{
  [Test]
  public void Ctor_Create_Table()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Table
    };

    //Act
    var withOrAsClause = new WithOrAsClause(Mock.Of<IServiceScopeFactory>(), statementContext);

    //Assert
    statementContext.Statement.Should().Be("CREATE TABLE ");
  }

  [Test]
  public void Ctor_Create_Stream()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    //Act
    var withOrAsClause = new WithOrAsClause(Mock.Of<IServiceScopeFactory>(), statementContext);

    //Assert
    statementContext.Statement.Should().Be("CREATE STREAM ");
  }

  [Test]
  public void Ctor_CreateOrReplace_Table()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Table
    };

    //Act
    var withOrAsClause = new WithOrAsClause(Mock.Of<IServiceScopeFactory>(), statementContext);

    //Assert
    statementContext.Statement.Should().Be("CREATE OR REPLACE TABLE ");
  }

  [Test]
  public void Ctor_CreateOrReplace_Stream()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Stream
    };

    //Act
    var withOrAsClause = new WithOrAsClause(Mock.Of<IServiceScopeFactory>(), statementContext);

    //Assert
    statementContext.Statement.Should().Be("CREATE OR REPLACE STREAM ");
  }

  private static readonly string TestTableName = "";

  [Test]
  public void Ctor_EntityName_CreateTableWithName()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      EntityName = TestTableName
    };

    //Act
    var withOrAsClause = new WithOrAsClause(Mock.Of<IServiceScopeFactory>(), statementContext);

    //Assert
    statementContext.Statement.Should().Be($"CREATE TABLE {TestTableName}");
  }
}