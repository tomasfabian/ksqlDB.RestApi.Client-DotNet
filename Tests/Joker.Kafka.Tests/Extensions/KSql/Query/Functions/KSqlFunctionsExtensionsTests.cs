using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Functions
{
  [TestClass]
  public class KSqlFunctionsExtensionsTests : TestBase
  {
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Like_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      var kSqlFunctions = KSqlFunctions.Instance.Like("", "");

      //Assert
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Trim_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      string kSqlFunctions = KSqlFunctions.Instance.Trim("");

      //Assert
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void LPad_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      var kSqlFunctions = KSqlFunctions.Instance.LPad("", 2, "");

      //Assert
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RPad_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      var kSqlFunctions = KSqlFunctions.Instance.RPad("", 2, "");

      //Assert
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Substring_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      var kSqlFunctions = KSqlFunctions.Instance.Substring("", 2, 2);

      //Assert
    }

    #region Date and time

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DateToString_ThrowsInvalidOperationException()
    {
      //Arrange

      //Act
      string kSqlFunctions = KSqlFunctions.Instance.DateToString(1, "");

      //Assert
    }

    #endregion
  }
}