using System.Linq;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context.Options
{
  [TestClass]
  public class KSqlDbJsonSerializerOptionsTests
  {
    [TestMethod]
    public void CreateInstance_PropertyNameCaseInsensitive()
    {
      //Arrange
      var options = KSqlDbJsonSerializerOptions.CreateInstance();

      //Act
      var propertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;

      //Assert
      propertyNameCaseInsensitive.Should().BeTrue();
    }

    [TestMethod]
    public void CreateInstance_Converters()
    {
      //Arrange
      var options = KSqlDbJsonSerializerOptions.CreateInstance();

      //Act
      var converters = options.Converters;

      //Assert
      converters.OfType<TimeSpanToStringConverter>().Any().Should().BeTrue();
    }
  }
}