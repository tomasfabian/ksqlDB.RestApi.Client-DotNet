using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Functions
{
  [TestClass]
  public class KSqlFunctionsTests : TestBase
  {
    [TestMethod]
    public void Instance_ReturnsSelf()
    {
      //Arrange

      //Act
      var kSqlFunctions = KSqlFunctions.Instance;

      //Assert
      kSqlFunctions.Should().NotBeNull();
      kSqlFunctions.Should().BeOfType<KSqlFunctions>();
    }

    [TestMethod]
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

    [TestMethod]
    public void KSqlFunctions_ReturnsInstanceOfFunctions()
    {
      //Arrange

      //Act
      var kSqlFunctions = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.Functions;

      //Assert
      kSqlFunctions.Should().BeOfType<KSqlFunctions>();
      kSqlFunctions.Should().BeSameAs(KSqlFunctions.Instance);
    }

    [TestMethod]
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
}