using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context
{
  [TestClass]
  public class KSqlDBContextTests : TestBase
  {
    [TestMethod]
    public void CreateStreamSet_Subscribe_KSqlDbProvidersRunWasCalled()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);

      //Act
      var streamSet = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      streamSet.Should().NotBeNull();
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void CreateStreamSet_Subscribe_QueryOptionsWereTakenFromContext()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl)
      {
        QueryStreamParameters =
        {
          ["auto.offset.reset"] = "latest"
        }
      };

      var context = new TestableDbProvider<string>(contextOptions);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(c => c["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
      
      subscription.Dispose();
    }

    [TestMethod]
    public void WithOffsetResetPolicy_Subscribe_QueryOptionsWereTakenFromContext()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);

      var context = new TestableDbProvider<string>(contextOptions)
      {
        RegisterKSqlQueryGenerator = false
      };

      //Act
      var subscription = context.CreateQueryStream<string>().WithOffsetResetPolicy(AutoOffsetReset.Latest).Subscribe(_ => {});

      //Assert
      // context.KSqldbProviderMock.Verify(c => c.Run<string>(It.Is<IQueryParameters>(c => c["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(c => c.AutoOffsetReset == AutoOffsetReset.Latest), It.IsAny<CancellationToken>()), Times.Once);

      subscription.Dispose();
    }

    [TestMethod]
    public void SetAutoOffsetReset_Subscribe_ProcessingGuarantee()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      contextOptions.SetAutoOffsetReset(AutoOffsetReset.Latest); 

      var context = new TestableDbProvider<string>(contextOptions);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(c => c["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void CreateStreamSet_Subscribe_ProcessingGuarantee()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      contextOptions.SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce); 

      var context = new TestableDbProvider<string>(contextOptions);

      //Act
      var subscription = context.CreateQueryStream<string>().Subscribe(_ => {});

      //Assert
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(c => c[KSqlDbConfigs.ProcessingGuarantee] == "exactly_once"), It.IsAny<CancellationToken>()), Times.Once);
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
      context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.Is<QueryStreamParameters>(parameters => parameters["auto.offset.reset"] == "latest"), It.IsAny<CancellationToken>()), Times.Once);
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
    public void CreateQueryStream_RawKSQL_ReturnAsyncEnumerable()
    {
      //Arrange
      string ksql = "SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

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
    public void CreateQuery_RawKSQL_ReturnAsyncEnumerable()
    {
      //Arrange
      string ksql = "SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

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

    [TestMethod]
    public async Task AddAndSaveChangesAsync()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);
      
      var entity = new Tweet();
      context.KSqlDbRestApiClientMock.Setup(c => c.ToInsertStatement(entity, null)).Returns(new KSqlDbStatement("Insert Into"));
      
      //Act
      context.Add(entity);
      await context.SaveChangesAsync();

      //Assert
      context.KSqlDbRestApiClientMock.Verify(c => c.ToInsertStatement(entity, null), Times.Once);
      context.KSqlDbRestApiClientMock.Verify(c => c.ExecuteStatementAsync(It.IsAny<KSqlDbStatement>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void AddTwice_SaveChangesAsyncWasNotCalled()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);
      var entity = new Tweet();
      context.KSqlDbRestApiClientMock.Setup(c => c.ToInsertStatement(entity, null)).Returns(new KSqlDbStatement("Insert Into"));
      
      //Act
      context.Add(entity);
      context.Add(entity);

      //Assert
      context.KSqlDbRestApiClientMock.Verify(c => c.ToInsertStatement(entity, null), Times.Exactly(2));
      context.KSqlDbRestApiClientMock.Verify(c => c.ExecuteStatementAsync(It.IsAny<KSqlDbStatement>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public void AddWithInsertProperties()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl); 
      var entity = new Tweet();
      var insertProperties = new InsertProperties();

      //Act
      context.Add(entity, insertProperties);

      //Assert
      context.KSqlDbRestApiClientMock.Verify(c => c.ToInsertStatement(entity, insertProperties), Times.Once);
    }

    [TestMethod]
    public async Task NothingWasAdded_SaveChangesAsync_WasNotCalled()
    {
      //Arrange
      var context = new TestableDbProvider<string>(TestParameters.KsqlDBUrl);
      
      //Act
      var response = await context.SaveChangesAsync();

      //Assert
      response.Should().BeNull();
    }
  }
}