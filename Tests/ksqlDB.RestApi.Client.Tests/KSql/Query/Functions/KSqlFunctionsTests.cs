using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Functions;

public class KSqlFunctionsTests : TestBase
{
  [Test]
  public void Instance_ReturnsSelf()
  {
    //Arrange

    //Act
    var kSqlFunctions = KSqlFunctions.Instance;

    //Assert
    kSqlFunctions.Should().NotBeNull();
    kSqlFunctions.Should().BeOfType<KSqlFunctions>();
  }

  [Test]
  public void K_ReturnsInstanceOfFunctions()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions;

    //Assert
    kSqlFunctions.Should().NotBeNull();
    kSqlFunctions.Should().BeOfType<KSqlFunctions>();
    kSqlFunctions.Should().BeSameAs(KSqlFunctions.Instance);
  }

  [Test]
  public void KSqlFunctions_ReturnsInstanceOfFunctions()
  {
    //Arrange

    //Act
    var kSqlFunctions = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.Functions;

    //Assert
    kSqlFunctions.Should().BeOfType<KSqlFunctions>();
    kSqlFunctions.Should().BeSameAs(KSqlFunctions.Instance);
  }

  [Test]
  public void KSqlF_ReturnsInstanceOfFunctions()
  {
    //Arrange

    //Act
    var kSqlFunctions = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.F;

    //Assert
    kSqlFunctions.Should().BeOfType<KSqlFunctions>();
    kSqlFunctions.Should().BeSameAs(KSqlFunctions.Instance);
  }
}
