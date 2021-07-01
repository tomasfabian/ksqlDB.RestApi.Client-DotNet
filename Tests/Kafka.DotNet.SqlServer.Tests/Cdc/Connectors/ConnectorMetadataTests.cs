using FluentAssertions;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.SqlServer.Tests.Cdc.Connectors
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
  }
}