using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Generators.Asserts;

public class AssertSchemaOptionsTests
{
  [Test]
  public void Ctor_SubjectAndIdWereNotDefined_ThrowsException()
  {
    //Arrange

    //Assert
    Assert.Throws<ArgumentException>(() => new AssertSchemaOptions(null));
  }
}