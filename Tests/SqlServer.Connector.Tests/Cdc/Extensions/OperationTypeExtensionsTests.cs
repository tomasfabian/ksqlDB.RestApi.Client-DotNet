using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Extensions;
using UnitTests;

namespace SqlServer.Connector.Tests.Cdc.Extensions
{
  [TestClass]
  public class OperationTypeExtensionsTests : TestBase
  {
    [TestMethod]
    public void ToChangeDataCaptureType_r_Read()
    {
      //Arrange
      string operation = "r";

      //Act
      var cdcTpe = operation.ToChangeDataCaptureType();

      //Assert
      cdcTpe.Should().Be(ChangeDataCaptureType.Read);
    }

    [TestMethod]
    public void ToChangeDataCaptureType_c_Created()
    {
      //Arrange
      string operation = "c";

      //Act
      var cdcTpe = operation.ToChangeDataCaptureType();

      //Assert
      cdcTpe.Should().Be(ChangeDataCaptureType.Created);
    }

    [TestMethod]
    public void ToChangeDataCaptureType_u_Updated()
    {
      //Arrange
      string operation = "u";

      //Act
      var cdcTpe = operation.ToChangeDataCaptureType();

      //Assert
      cdcTpe.Should().Be(ChangeDataCaptureType.Updated);
    }

    [TestMethod]
    public void ToChangeDataCaptureType_d_Deleted()
    {
      //Arrange
      string operation = "d";

      //Act
      var cdcTpe = operation.ToChangeDataCaptureType();

      //Assert
      cdcTpe.Should().Be(ChangeDataCaptureType.Deleted);
    }
  }
}