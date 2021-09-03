using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.Tests.Infrastructure.Extensions
{
  [TestClass]
  public class TypeExtensionsTests
  {
    [TestMethod]
    public void IsAnonymousType()
    {
      //Arrange

      //Act

      //Assert
    }

    [TestMethod]
    public void IsStruct()
    {
      //Arrange
      var type = typeof(ValueTask);

      //Act
      var typeDefinition = type.IsStruct();

      //Assert
      typeDefinition.Should().BeTrue();
    }

    [TestMethod]
    public void IsStruct_PrimitiveType_ReturnsFalse()
    {
      //Arrange
      var type = typeof(int);

      //Act
      var typeDefinition = type.IsStruct();

      //Assert
      typeDefinition.Should().BeFalse();
    }

    enum TestTypes{}

    [TestMethod]
    public void IsStruct_EnumType_ReturnsFalse()
    {
      //Arrange
      var type = typeof(TestTypes);

      //Act
      var typeDefinition = type.IsStruct();

      //Assert
      typeDefinition.Should().BeFalse();
    }

    [TestMethod]
    public void IsDictionary()
    {
      //Arrange
      var type = typeof(IDictionary<string, int>);

      //Act
      var typeDefinition = type.IsDictionary();

      //Assert
      typeDefinition.Should().BeTrue();
    }

    [TestMethod]
    public void GetEnumerableTypeDefinition_FindsEnumerableType()
    {
      //Arrange
      var type = typeof(TestEnumerable);

      //Act
      var typeDefinition = type.GetEnumerableTypeDefinition();

      //Assert
      typeDefinition.Should().Contain(typeof(IEnumerable<string>));
    }

    [TestMethod]
    public void GetEnumerableTypeDefinition_ReturnsNullForNonEnumerableTypes()
    {
      //Arrange
      var type = typeof(int);

      //Act
      var typeDefinition = type.GetEnumerableTypeDefinition();

      //Assert
      typeDefinition.Should().BeEmpty();
    }

    class TestEnumerable : List<string>
    {

    }
  }
}