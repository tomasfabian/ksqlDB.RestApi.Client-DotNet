using FluentAssertions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.Infrastructure.Extensions
{
  [TestClass]
  public class EnumerableExtensionsTests
  {
    [TestMethod]
    public void IsOneOfFollowing()
    {
      //Arrange

      //Act
      var isOneOf = 1.IsOneOfFollowing(1, 2, 3);

      //Assert
      isOneOf.Should().BeTrue();
    }

    [TestMethod]
    public void IsOneOfFollowing_ReturnsFalse()
    {
      //Arrange

      //Act
      var isOneOf = 4.IsOneOfFollowing(1, 2, 3);

      //Assert
      isOneOf.Should().BeFalse();
    }

    [TestMethod]
    public void IsNotOneOfFollowing()
    {
      //Arrange

      //Act
      var isNotOneOf = 4.IsNotOneOfFollowing(1, 2, 3);

      //Assert
      isNotOneOf.Should().BeTrue();
    }

    [TestMethod]
    public void IsNotOneOfFollowing_ReturnsFalse()
    {
      //Arrange

      //Act
      var isNotOneOf = 1.IsNotOneOfFollowing(1, 2, 3);

      //Assert
      isNotOneOf.Should().BeFalse();
    }
  }
}