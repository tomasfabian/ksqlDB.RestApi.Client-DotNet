using System.Text.Json;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters
{
  public class QueryEndpointParametersTests
  {
    private const string BooleanPropertyName = "ksql.query.pull.table.scan.enabled";

    [Test]
    public void BooleanProperty_IsSerializedAsJsonBoolean()
    {
      //Arrange
      var parameters = new PullQueryParameters
      {
        Sql = "Select",
        Properties = { [BooleanPropertyName] = true }
      };

      //Act
      var json = JsonSerializer.Serialize(parameters);

      //Assert
      using var document = JsonDocument.Parse(json);
      document.RootElement.GetProperty("streamsProperties").GetProperty(BooleanPropertyName)
        .ValueKind.Should().Be(JsonValueKind.True);
    }

    [Test]
    public void Clone_PreservesBooleanProperty()
    {
      //Arrange
      var source = new PullQueryParameters
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
