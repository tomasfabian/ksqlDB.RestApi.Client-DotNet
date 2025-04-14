using ksqlDB.RestApi.Client.KSql.Query.Functions;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Functions;

[TestFixture]
public class KSqlFunctionsExtensionsTests : TestBase
{
  [Test]
  public void Like_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.Like("", ""));
  }

  [Test]
  public void Trim_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.Trim(""));
  }

  [Test]
  public void LPad_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.LPad("", 2, ""));
  }

  [Test]
  public void RPad_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.RPad("", 2, ""));
  }

  [Test]
  public void Substring_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.Substring("", 2, 2));
  }

  #region Date and time

  [Test]
  public void DateToString_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(() => KSqlFunctions.Instance.DateToString(1, ""));
  }

  #endregion
}
