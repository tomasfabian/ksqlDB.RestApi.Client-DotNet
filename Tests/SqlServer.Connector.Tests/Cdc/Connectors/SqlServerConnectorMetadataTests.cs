using FluentAssertions;
using NUnit.Framework;
using SqlServer.Connector.Cdc.Connectors;
using UnitTests;

namespace SqlServer.Connector.Tests.Cdc.Connectors;

[TestFixture]
public class SqlServerConnectorMetadataTests : TestBase<SqlServerConnectorMetadata>
{
  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    string connectionString =
      "Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

    ClassUnderTest = new SqlServerConnectorMetadata(connectionString);
  }

  [Test]
  public void ConnectorClass()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.ConnectorClass.Should().Be("io.debezium.connector.sqlserver.SqlServerConnector");
  }

  [Test]
  public void DatabaseHostnameName()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseHostname.Should().Be("127.0.0.1");
  }

  [Test]
  public void DatabaseUser()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseUser.Should().Be("SA");
  }

  [Test]
  public void DefaultCtor_Port()
  {
    //Arrange
    ClassUnderTest = new SqlServerConnectorMetadata();

    //Act

    //Assert
    ClassUnderTest.DatabasePort.Should().Be("1433");
  }

  [Test]
  public void Port()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabasePort.Should().Be("1433");
  }

  [Test]
  public void DatabasePassword()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabasePassword.Should().Be("<YourNewStrong@Passw0rd>");
  }

  [Test]
  public void DatabaseDbname()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseDbname.Should().Be("Sensors");
  }

  [Test]
  public void TrySetDatabaseHistoryKafkaTopic()
  {
    //Arrange
    ClassUnderTest.DatabaseServerName = "GAIA";

    //Act
    ClassUnderTest.TrySetDatabaseHistoryKafkaTopic();

    //Assert
    ClassUnderTest.DatabaseHistoryKafkaTopic.Should().Be($"dbhistory.{ClassUnderTest.DatabaseServerName}");
  }

  [Test]
  public void TrySetConnectorName()
  {
    //Arrange

    //Act
    ClassUnderTest.TrySetConnectorName();

    //Assert
    ClassUnderTest.Name.Should().Be($"{ClassUnderTest.DatabaseDbname}-connector");
  }

  [Test]
  public void KafkaBootstrapServers()
  {
    //Arrange
    string bootstrapServers = "localhost:9092,broker01:29092";

    //Act
    ClassUnderTest.KafkaBootstrapServers = bootstrapServers;

    //Assert
    ClassUnderTest.KafkaBootstrapServers.Should().Be(bootstrapServers);
  }
}
