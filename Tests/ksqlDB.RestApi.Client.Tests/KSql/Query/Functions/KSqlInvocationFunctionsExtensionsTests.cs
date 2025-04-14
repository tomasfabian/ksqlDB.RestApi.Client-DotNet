using ksqlDB.RestApi.Client.KSql.Query.Functions;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Functions;

[TestFixture]
public class KSqlInvocationFunctionsExtensionsTests : TestBase
{
  [Test]
  public void Transform_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Transform(Array.Empty<int>(), c => c)
    );
  }

  [Test]
  public void Filter_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Filter(Array.Empty<int>(), c => true)
    );
  }

  [Test]
  public void Reduce_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Reduce(Array.Empty<int>(), 0, (x, y) => x)
    );
  }

  [Test]
  public void TransformMap_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Transform(new Dictionary<int, string>(), c => c)
    );
  }

  [Test]
  public void FilterMap_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Filter(new Dictionary<int, string>(), c => true)
    );
  }

  [Test]
  public void ReduceMap_ThrowsInvalidOperationException()
  {
    //Assert
    Assert.Throws<InvalidOperationException>(
      () => K.Functions.Reduce(new Dictionary<int, string>(), 0, (x, y) => x)
    );
  }
}
