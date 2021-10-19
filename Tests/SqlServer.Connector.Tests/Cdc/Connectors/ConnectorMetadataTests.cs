using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc.Connectors;
using UnitTests;

namespace SqlServer.Connector.Tests.Cdc.Connectors
{
  [TestClass]
  public class ConnectorMetadataTests : TestBase<ConnectorMetadata>
  {    
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new ConnectorMetadata();
    }

    [TestMethod]
    public void JsonKeyConverter()
    {
      //Arrange

      //Act
      ClassUnderTest.SetJsonKeyConverter();

      //Assert
      ClassUnderTest.KeyConverter.Should().Be("org.apache.kafka.connect.json.JsonConverter");
    }

    [TestMethod]
    public void ValueConverter()
    {
      //Arrange

      //Act
      ClassUnderTest.SetJsonValueConverter();

      //Assert
      ClassUnderTest.ValueConverter.Should().Be("org.apache.kafka.connect.json.JsonConverter");
    }

    [TestMethod]
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
}