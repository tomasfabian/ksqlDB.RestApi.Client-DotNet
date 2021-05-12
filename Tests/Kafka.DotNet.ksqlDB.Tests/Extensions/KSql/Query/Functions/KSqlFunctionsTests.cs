using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Functions
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
  }
}