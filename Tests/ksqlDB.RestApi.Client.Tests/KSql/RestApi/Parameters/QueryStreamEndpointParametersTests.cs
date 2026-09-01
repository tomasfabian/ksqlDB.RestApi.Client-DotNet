using System.Text.Json;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters
{
  public class QueryStreamEndpointParametersTests
  {
    private const string BooleanPropertyName = "ksql.query.pull.table.scan.enabled";

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

    [Test]
    public void BooleanProperty_IsSerializedAsJsonBoolean()
    {
      //Arrange
      var parameters = new PullQueryStreamParameters
      {
        Sql = "Select",
        Properties = { [BooleanPropertyName] = true }
      };

      //Act
      var json = JsonSerializer.Serialize(parameters);

      //Assert
      using var document = JsonDocument.Parse(json);
      document.RootElement.GetProperty("properties").GetProperty(BooleanPropertyName)
        .ValueKind.Should().Be(JsonValueKind.True);
    }

    [Test]
    public void Clone_PreservesBooleanProperty()
    {
      //Arrange
      var source = new PullQueryStreamParameters
      {
        Sql = "Select",
        Properties = { [BooleanPropertyName] = true }
      };

      //Act
      var clone = source.Clone();

      //Assert
      clone.Properties[BooleanPropertyName].Should().Be(true);
    }
  }
}
