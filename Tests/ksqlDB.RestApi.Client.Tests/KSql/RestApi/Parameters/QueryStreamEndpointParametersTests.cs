using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters
{
  public class QueryStreamEndpointParametersTests
  {
    [Test]
    public void Clone()
    {
      //Arrange
      var source = new QueryStreamParameters
      {
        Sql = "Select",
        ["key"] = "value"
      };

      //Act
      var clone = source.Clone();

      //Assert
      clone.Sql.Should().BeEquivalentTo(source.Sql);
      clone.Properties.Count.Should().Be(source.Properties.Count);
    }

    [Test]
    public void QueryStreamParameters_AutoOffsetReset_CorrectKeyWasUsed()
    {
      //Arrange
      var source = new QueryStreamParameters
      {
        AutoOffsetReset = AutoOffsetReset.Earliest
      };

      //Act
      var clone = source.Clone();

      //Assert
      clone.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().Be(nameof(AutoOffsetReset.Earliest).ToLower());
    }

    [Test]
    public void QueryParameters_AutoOffsetReset_CorrectKeyWasUsed()
    {
      //Arrange
      var source = new QueryParameters
      {
        AutoOffsetReset = AutoOffsetReset.Latest
      };

      //Act
      var clone = source.Clone();

      //Assert
      clone.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().Be(nameof(AutoOffsetReset.Latest).ToLower());
    }
  }
}
