using FluentAssertions;
using SqlServer.Connector.Cdc.Connectors;
using NUnit.Framework;
using UnitTests;

namespace SqlServer.Connector.Tests.Cdc.Connectors;

[TestFixture]
public class ConnectorMetadataTests : TestBase<ConnectorMetadata>
{
  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new ConnectorMetadata();
  }

  [Test]
  public void JsonKeyConverter()
  {
    //Arrange

    //Act
    ClassUnderTest.SetJsonKeyConverter();

    //Assert
    ClassUnderTest.KeyConverter.Should().Be("org.apache.kafka.connect.json.JsonConverter");
  }

  [Test]
  public void ValueConverter()
  {
    //Arrange

    //Act
    ClassUnderTest.SetJsonValueConverter();

    //Assert
    ClassUnderTest.ValueConverter.Should().Be("org.apache.kafka.connect.json.JsonConverter");
  }

  [Test]
  public void SetProperty()
  {
    //Arrange
    var sqlServerConnector = "io.debezium.connector.sqlserver.SqlServerConnector";

    //Act
    ClassUnderTest.SetProperty("connector.class", sqlServerConnector);

    //Assert
    ClassUnderTest.ConnectorClass.Should().Be(sqlServerConnector);
  }
}
