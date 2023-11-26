using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context.Options;

public class KSqlDbJsonSerializerOptionsTests
{
  [Test]
  public void CreateInstance_PropertyNameCaseInsensitive()
  {
    //Arrange
    var options = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var propertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;

    //Assert
    propertyNameCaseInsensitive.Should().BeTrue();
  }

  [Test]
  public void CreateInstance_Converters_ContainsTimeSpanToStringConverter()
  {
    //Arrange
    var options = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var converters = options.Converters;

    //Assert
    converters.OfType<TimeSpanToStringConverter>().Any().Should().BeTrue();
  }

  [Test]
  public void CreateInstance_Converters_ContainsJsonConverterGuid()
  {
    //Arrange
    var options = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var converters = options.Converters;

    //Assert
    converters.OfType<JsonConverterGuid>().Any().Should().BeTrue();
  }
}
