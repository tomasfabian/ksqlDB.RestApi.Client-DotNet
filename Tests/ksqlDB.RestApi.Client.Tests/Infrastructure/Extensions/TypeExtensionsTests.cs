using System.Collections;
using System.Text;
using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.Infrastructure.Extensions;

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
  public void IsList()
  {
    //Arrange
    var type = typeof(List<string>);

    //Act
    var typeDefinition = type.IsList();

    //Assert
    typeDefinition.Should().BeTrue();
  }

  [TestMethod]
  public void IsList_Interface()
  {
    //Arrange
    var type = typeof(IList<string>);

    //Act
    var typeDefinition = type.IsList();

    //Assert
    typeDefinition.Should().BeTrue();
  }

  [TestMethod]
  public void IsList_PrimitiveType_ReturnsFalse()
  {
    //Arrange
    var type = typeof(string);

    //Act
    var typeDefinition = type.IsList();

    //Assert
    typeDefinition.Should().BeFalse();
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

  #region GetEnumerableTypeDefinition
    
  [TestMethod]
  public void GetEnumerableTypeDefinition_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(IEnumerable);

    //Act
    var typeDefinition = type.GetEnumerableTypeDefinition();

    //Assert
    typeDefinition.Should().Contain(typeof(IEnumerable));
  }
    
  class TestEnumerable : IEnumerable<string>
  {
    public IEnumerator<string> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  [TestMethod]
  public void GetEnumerableTypeDefinition_EnumerableOfStringBaseType_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(TestEnumerable);

    //Act
    var typeDefinition = type.GetEnumerableTypeDefinition();

    //Assert
    typeDefinition.Should().Contain(typeof(IEnumerable<string>));
  }

  class TestList : List<string>
  {
  }

  [TestMethod]
  public void GetEnumerableTypeDefinition_ListBaseType_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(TestList);

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

  [TestMethod]
  public void GetEnumerableOfStringTypeDefinition_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(IEnumerable<string>);

    //Act
    var typeDefinitions = type.GetEnumerableTypeDefinition();

    //Assert
    typeDefinitions.Should().Contain(typeof(IEnumerable<string>));
  }

  [TestMethod]
  public void GetEnumerableOfStringTypeDefinition_IList_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(IList<string>);

    //Act
    var typeDefinitions = type.GetEnumerableTypeDefinition();

    //Assert
    typeDefinitions.Should().Contain(typeof(IEnumerable<string>));
  }

  [TestMethod]
  public void GetEnumerableOfStringTypeDefinition_List_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(List<string>);

    //Act
    var typeDefinitions = type.GetEnumerableTypeDefinition();

    //Assert
    typeDefinitions.Should().Contain(typeof(IEnumerable<string>));
  }

  #endregion

  [TestMethod]
  public void IsKsqlGrouping_List_FindsEnumerableType()
  {
    //Arrange
    var type = typeof(List<string>);

    //Act
    var isKsqlGrouping = type.IsKsqlGrouping();

    //Assert
    isKsqlGrouping.Should().BeFalse();
  }

  [TestMethod]
  public void IsKsqlGrouping_KsqlGrouping_ReturnsTrue()
  {
    //Arrange
    var type = typeof(IKSqlGrouping<int, string>);

    //Act
    var isKsqlGrouping = type.IsKsqlGrouping();

    //Assert
    isKsqlGrouping.Should().BeTrue();
  }

  [TestMethod]
  public void ExtractName_Type()
  {
    //Arrange
    var type = typeof(TestList);

    //Act
    var name = type.ExtractTypeName();

    //Assert
    name.Should().Be(nameof(TestList));
  }

  [TestMethod]
  public void ExtractName_GenericType()
  {
    //Arrange
    var type = typeof(List<string>);

    //Act
    var name = type.ExtractTypeName();

    //Assert
    name.Should().Be("List");
  }

  [TestMethod]
  public void TryGetAttribute()
  {
    //Arrange
    var type = typeof(Test);

    //Act
    var attribute = type.GetProperty(nameof(Test.Key)).TryGetAttribute<KeyAttribute>();

    //Assert
    attribute.Should().NotBeNull();
    attribute.Should().BeOfType<KeyAttribute>();
  }

  private record MySensor
  {
    [JsonPropertyName("SensorId")]
    public string SensorId2 { get; set; } = null!;

    public string Title { get; set; } = null!;
  }

  [TestMethod]
  public void GetMemberName()
  {
    //Arrange
    var type = typeof(MySensor);
    var member = type.GetProperty(nameof(MySensor.Title));

    //Act
    var memberName = member.GetMemberName();

    //Assert
    memberName.Should().Be(nameof(MySensor.Title));
  }

  [TestMethod]
  public void GetMemberName_JsonPropertyNameOverride()
  {
    //Arrange
    var type = typeof(MySensor);

    //Act
    var member = type.GetProperty(nameof(MySensor.SensorId2));

    var memberName = member.GetMemberName();

    //Assert
    memberName.Should().Be("SensorId");
  }
}
