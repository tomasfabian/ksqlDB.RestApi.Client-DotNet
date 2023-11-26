using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Moq;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context;

public class ChangesCacheTests
{
  private ChangesCache ClassUnderTest { get; set; } = null!;

  private Mock<IKSqlDbRestApiClient> kSqlDbRestApiClientMock = null!;

  [SetUp]
  public void TestInitialize()
  {
    kSqlDbRestApiClientMock = new Mock<IKSqlDbRestApiClient>();

    ClassUnderTest = new ChangesCache();
  }

  [Test]
  public async Task AddTwice_SaveChangesAsyncWasNotCalled()
  {
    //Arrange
    ClassUnderTest.Enqueue(new KSqlDbStatement("Insert 1;"));
    ClassUnderTest.Enqueue(new KSqlDbStatement("Insert 2;"));

    //Act
    var result = await ClassUnderTest.SaveChangesAsync(kSqlDbRestApiClientMock.Object, new CancellationToken());

    //Assert
    string expectedSql = @"Insert 1;
Insert 2;
";
    kSqlDbRestApiClientMock.Verify(c => c.ExecuteStatementAsync(It.Is<KSqlDbStatement>(c => c.Sql == expectedSql), It.IsAny<CancellationToken>()), Times.Once);
  }
}
