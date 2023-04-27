using ksqlDB.RestApi.Client.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Functions;

[TestClass]
public class KSqlInvocationFunctionsExtensionsTests : TestBase
{
  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void Transform_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Transform(Array.Empty<int>(), c => c);

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void Filter_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Filter(Array.Empty<int>(), c => true);

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void Reduce_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Reduce(Array.Empty<int>(), 0, (x, y) => x);

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void TransformMap_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Transform(new Dictionary<int, string>(), c => c);

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void FilterMap_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Filter(new Dictionary<int, string>(), c => true);

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void ReduceMap_ThrowsInvalidOperationException()
  {
    //Arrange

    //Act
    var kSqlFunctions = K.Functions.Reduce(new Dictionary<int, string>(), 0, (x, y) => x);

    //Assert
  }
}
