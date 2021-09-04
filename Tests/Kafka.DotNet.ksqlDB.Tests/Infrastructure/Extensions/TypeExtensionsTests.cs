using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
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
      var type = new { a = "1"}.GetType();

      //Act
      var isAnonymousType = type.IsAnonymousType();

      //Assert
      isAnonymousType.Should().BeTrue();
    }

    [TestMethod]
    public void IsAnonymousType_Class_ReturnsFalse()
    {
      //Arrange
      var type = typeof(StringBuilder);

      //Act
      var isAnonymousType = type.IsAnonymousType();

      //Assert
      isAnonymousType.Should().BeFalse();
    }

    [TestMethod]
    public void TryFindProviderAncestor()
    {
      //Arrange
      var type = typeof(KQueryStreamSet<string>);

      //Act
      var providerAncestorType = type.TryFindProviderAncestor();

      //Assert
      providerAncestorType.Should().Be(typeof(KSet));
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
    public void HasKey_PropertyIsNotAnnotatedWithAKeyAttribute_ReturnsFalse()
    {
      //Arrange
      var type = typeof(Test).GetProperty(nameof(Test.Value));

      //Act
      var hasKey = type.HasKey();

      //Assert
      hasKey.Should().BeFalse();
    }

    private record Test
    {
      [Key]
      public int Key { get; set; }

      public int Value { get; set; }
    }

    [TestMethod]
    public void HasKey()
    {
      //Arrange
      var type = typeof(Test).GetProperty(nameof(Test.Key));

      //Act
      var hasKey = type.HasKey();

      //Assert
      hasKey.Should().BeTrue();
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