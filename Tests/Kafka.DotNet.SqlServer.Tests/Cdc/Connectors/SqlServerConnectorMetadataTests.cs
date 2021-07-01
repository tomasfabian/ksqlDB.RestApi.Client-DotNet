using FluentAssertions;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.SqlServer.Tests.Cdc.Connectors
{
  [TestClass]
  public class SqlServerConnectorMetadataTests : TestBase<SqlServerConnectorMetadata>
  {
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      string connectionString =
        "Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

      ClassUnderTest = new SqlServerConnectorMetadata(connectionString);
    }

    [TestMethod]
    public void ConnectorClass()
    {
      //Arrange

      //Act

      //Assert
      ClassUnderTest.ConnectorClass.Should().Be("io.debezium.connector.sqlserver.SqlServerConnector");
    }

    [TestMethod]
    public void DatabaseHostnameName()
    {
      //Arrange

      //Act

      //Assert
      ClassUnderTest.DatabaseHostname.Should().Be("127.0.0.1");
    }

    [TestMethod]
    public void DatabaseUser()
    {
      //Arrange

      //Act

      //Assert
      ClassUnderTest.DatabaseUser.Should().Be("SA");
    }

    [TestMethod]
    public void DefaultCtor_Port()
    {
      //Arrange
      ClassUnderTest = new SqlServerConnectorMetadata();

      //Act

      //Assert
      ClassUnderTest.DatabasePort.Should().Be("1433");
    }

    [TestMethod]
    public void Port()
    {
      //Arrange

      //Act

      //Assert
      ClassUnderTest.DatabasePort.Should().Be("1433");
    }
  }
}