using FluentAssertions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Infrastructure.Extensions;

public class EnumerableExtensionsTests
{
  [Test]
  public void IsOneOfFollowing()
  {
    //Arrange

    //Act
    var isOneOf = 1.IsOneOfFollowing(1, 2, 3);

    //Assert
    isOneOf.Should().BeTrue();
  }

  [Test]
  public void IsOneOfFollowing_ReturnsFalse()
  {
    //Arrange

    //Act
    var isOneOf = 4.IsOneOfFollowing(1, 2, 3);

    //Assert
    isOneOf.Should().BeFalse();
  }

  [Test]
  public void IsNotOneOfFollowing()
  {
    //Arrange

    //Act
    var isNotOneOf = 4.IsNotOneOfFollowing(1, 2, 3);

    //Assert
    isNotOneOf.Should().BeTrue();
  }

  [Test]
  public void IsNotOneOfFollowing_ReturnsFalse()
  {
    //Arrange

    //Act
    var isNotOneOf = 1.IsNotOneOfFollowing(1, 2, 3);

    //Assert
    isNotOneOf.Should().BeFalse();
  }
}
