using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Context
{
  [TestClass]
  public class KSqlDBContextTests : TestBase
  {
    [TestMethod]
    public void CreateStreamSet_Subscribe_KSqldbProvidersRunWasCalled()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      var streamSet = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      streamSet.Should().NotBeNull();
      context.KSqldbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void CreateStreamSet_Subscribe_QueryOptionsWereTakenFromContext()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      contextOptions.QueryStreamParameters["auto.offset.reset"] = "latest";

      var context = new TestableDbProvider<string>(contextOptions);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      context.KSqldbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(c => c["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void CreateStreamSet_CalledMultipleTimes_KSqlQueryGeneratorBuildKSqlWasNotCalled()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      context.KSqlQueryGenerator.Verify(c => c.BuildKSql(It.IsAny<Expression>(), It.IsAny<QueryContext>()), Times.Once);
    }

    [TestMethod]
    public void CreateStreamSet_Subscribe_KSqlQueryGenerator()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      contextOptions.QueryStreamParameters["auto.offset.reset"] = "latest";

      var context = new TestableDbProvider<string>(contextOptions);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {}, e => {});

      //Assert
      context.KSqldbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(parameters => parameters["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DisposeAsync_ServiceProviderIsNull_ContextWasDisposed()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      await context.DisposeAsync().ConfigureAwait(false);

      //Assert
      context.IsDisposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task DisposeAsync_ServiceProviderWasBuilt_ContextWasDisposed()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);
      context.CreateQueryStream<string>();

      //Act
      await context.DisposeAsync().ConfigureAwait(false);

      //Assert
      context.IsDisposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task CreateQueryStream_RawKSQL_ReturnAsyncEnumerable()
    {
      //Arrange
      string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

      QueryStreamParameters queryStreamParameters = new QueryStreamParameters
      {
        Sql = ksql,
        [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
      };

      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      var source = context.CreateQueryStream<string>(queryStreamParameters);

      //Assert
      source.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateQuery_RawKSQL_ReturnAsyncEnumerable()
    {
      //Arrange
      string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

      QueryParameters queryParameters = new QueryParameters
      {
        Sql = ksql,
        [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
      };

      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      var source = context.CreateQuery<string>(queryParameters);

      //Assert
      source.Should().NotBeNull();
    }
  }
}