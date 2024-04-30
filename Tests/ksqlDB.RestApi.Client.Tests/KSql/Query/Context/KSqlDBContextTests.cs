using System.Linq.Expressions;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using UnitTests;
using EndpointType = ksqlDB.RestApi.Client.KSql.Query.Options.EndpointType;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context;

public class KSqlDBContextTests : TestBase
{
  [Test]
  public void CreatePushQuery_Subscribe_KSqlDbProvidersRunWasCalled()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { });

    //Assert
    subscription.Should().NotBeNull();
    context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void CreateStreamSet_Subscribe_QueryOptionsWereTakenFromContext()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
    {
      QueryStreamParameters =
      {
        [QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Latest.ToString()
      }
    };

    var context = new TestableDbProvider<string>(contextOptions);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { });

    //Assert
    context.KSqlDbProviderMock.Verify(
      c => c.Run<string>(It.Is<QueryStreamParameters>(c => c[QueryParameters.AutoOffsetResetPropertyName] == "Latest"),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public void WithOffsetResetPolicy_Subscribe_QueryOptionsWereTakenFromContext()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);

    var context = new TestableDbProvider<string>(contextOptions)
    {
      RegisterKSqlQueryGenerator = false
    };

    //Act
    var subscription = context.CreatePushQuery<string>().WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Subscribe(_ => { });

    //Assert
    context.KSqlDbProviderMock.Verify(
      c => c.Run<string>(It.Is<QueryStreamParameters>(c => c.AutoOffsetReset == AutoOffsetReset.Latest),
        It.IsAny<CancellationToken>()), Times.Once);

    subscription.Dispose();
  }

  [Test]
  public void SetAutoOffsetReset_Subscribe_ProcessingGuarantee()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);
    contextOptions.SetAutoOffsetReset(AutoOffsetReset.Latest);

    var context = new TestableDbProvider<string>(contextOptions);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { });

    //Assert
    context.KSqlDbProviderMock.Verify(
      c => c.Run<string>(It.Is<QueryStreamParameters>(c => c["auto.offset.reset"] == "latest"),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public void CreateStreamSet_Subscribe_ProcessingGuarantee()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);
    contextOptions.SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce);

    var context = new TestableDbProvider<string>(contextOptions);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { });

    //Assert
    context.KSqlDbProviderMock.Verify(
      c => c.Run<string>(It.Is<QueryStreamParameters>(c => c[KSqlDbConfigs.ProcessingGuarantee] == "exactly_once"),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public void CreateStreamSet_CalledMultipleTimes_KSqlQueryGeneratorBuildKSqlWasNotCalled()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { });

    //Assert
    context.KSqlQueryGenerator.Verify(c => c.BuildKSql(It.IsAny<Expression>(), It.IsAny<QueryContext>()), Times.Once);
  }

  [Test]
  public void CreateStreamSet_Subscribe_KSqlQueryGenerator()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
    {
      QueryStreamParameters =
      {
        ["auto.offset.reset"] = "latest"
      }
    };

    var context = new TestableDbProvider<string>(contextOptions);

    //Act
    using var subscription = context.CreatePushQuery<string>().Subscribe(_ => { }, e => { });

    //Assert
    context.KSqlDbProviderMock.Verify(
      c => c.Run<string>(It.Is<QueryStreamParameters>(parameters => parameters["auto.offset.reset"] == "latest"),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public async Task DisposeAsync_ServiceProviderIsNull_ContextWasDisposed()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    await context.DisposeAsync().ConfigureAwait(false);

    //Assert
    context.IsDisposed.Should().BeTrue();
  }

  [Test]
  public async Task DisposeAsync_ServiceProviderWasBuilt_ContextWasDisposed()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);
    context.CreatePushQuery<string>();

    //Act
    await context.DisposeAsync().ConfigureAwait(false);

    //Assert
    context.IsDisposed.Should().BeTrue();
  }

  [Test]
  public void CreatePushQuery_RawKSQL_ReturnAsyncEnumerable()
  {
    //Arrange
    string ksql = "SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    var source = context.CreatePushQuery<string>(queryStreamParameters);

    //Assert
    source.Should().NotBeNull();
  }


  [Test]
  public void CreatePushQuery_InvalidQueryParameterTypeThrows()
  {
    //Arrange
    string ksql = "SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

    var queryParameters = new QueryParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Assert
    Assert.Throws<InvalidOperationException>(() =>
    {
      //Act
      context.CreatePushQuery<string>(queryParameters);
    });
  }

  [Test]
  public void CreateQuery_RawKSQL_ReturnAsyncEnumerable()
  {
    //Arrange
    string ksql = "SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

    QueryStreamParameters queryParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    var source = context.CreatePushQuery<string>(queryParameters);

    //Assert
    source.Should().NotBeNull();
  }

  [Test]
  public async Task AddAndSaveChangesAsync()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    var entity = new Tweet();
    context.KSqlDbRestApiClientMock.Setup(c => c.ToInsertStatement(It.IsAny<InsertValues<Tweet>>(), null))
      .Returns(new KSqlDbStatement("Insert Into"));

    //Act
    context.Add(entity);
    await context.SaveChangesAsync();

    //Assert
    context.KSqlDbRestApiClientMock.Verify(
      c => c.ToInsertStatement(It.Is<InsertValues<Tweet>>(c => c.Entity == entity), null), Times.Once);
    context.KSqlDbRestApiClientMock.Verify(
      c => c.ExecuteStatementAsync(It.IsAny<KSqlDbStatement>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public void AddTwice_SaveChangesAsyncWasNotCalled()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);
    var entity = new Tweet();
    context.KSqlDbRestApiClientMock.Setup(c => c.ToInsertStatement(It.IsAny<InsertValues<Tweet>>(), null))
      .Returns(new KSqlDbStatement("Insert Into"));

    //Act
    context.Add(entity);
    context.Add(entity);

    //Assert
    context.ChangesCache.Count.Should().Be(2);

    context.KSqlDbRestApiClientMock.Verify(
      c => c.ToInsertStatement(It.Is<InsertValues<Tweet>>(c => c.Entity == entity), null), Times.Exactly(2));
    context.KSqlDbRestApiClientMock.Verify(
      c => c.ExecuteStatementAsync(It.IsAny<KSqlDbStatement>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Test]
  public void Add_InsertValues_WereCached()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    var entity = new Tweet();
    var insertValues = new InsertValues<Tweet>(entity);
    context.KSqlDbRestApiClientMock.Setup(c => c.ToInsertStatement(insertValues, null))
      .Returns(new KSqlDbStatement("Insert Into"));

    //Act
    context.Add(insertValues);

    //Assert
    context.ChangesCache.Count.Should().Be(1);
  }

  [Test]
  public void AddWithInsertProperties()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);
    var entity = new Tweet();
    var insertProperties = new InsertProperties();

    //Act
    context.Add(entity, insertProperties);

    //Assert
    context.KSqlDbRestApiClientMock.Verify(c => c.ToInsertStatement(It.IsAny<InsertValues<Tweet>>(), insertProperties),
      Times.Once);
  }

  [Test]
  public async Task NothingWasAdded_SaveChangesAsync_WasNotCalled()
  {
    //Arrange
    var context = new TestableDbProvider<string>(TestParameters.KsqlDbUrl);

    //Act
    var response = await context.SaveChangesAsync();

    //Assert
    response.Should().BeNull();
  }

  [Test]
  public void DependenciesForQueryEndpointTypeWereConfigured()
  {
    //Arrange
    KSqlDBContextOptions contextOptions = new(TestParameters.KsqlDbUrl)
    {
      EndpointType = EndpointType.Query
    };
    var context = new KSqlDBContext(contextOptions);

    _ = context.CreatePushQuery<int>();

    var serviceProvider = context.ServiceCollection
      .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

    //Act
    var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    var queryDbProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IKSqlDbProvider>();
    var pushQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbParameters>();
    var pullQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IPullQueryParameters>();

    //Assert
    queryDbProvider.Should().BeOfType<KSqlDbQueryProvider>();
    pushQueryParameters.Should().BeOfType<QueryParameters>();
    pullQueryParameters.Should().BeOfType<PullQueryParameters>();
  }

  [Test]
  public void DependenciesForQueryStreamEndpointTypeWereConfigured()
  {
    //Arrange
    KSqlDBContextOptions contextOptions = new(TestParameters.KsqlDbUrl)
    {
      EndpointType = EndpointType.QueryStream
    };
    var context = new KSqlDBContext(contextOptions);

    _ = context.CreatePushQuery<int>();

    var serviceProvider = context.ServiceCollection
      .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

    //Act
    var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    var queryDbProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbProvider>();
    var pushQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbParameters>();
    var pullQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IPullQueryParameters>();

    //Assert
    queryDbProvider.Should().BeOfType<KSqlDbQueryStreamProvider>();
    pushQueryParameters.Should().BeOfType<QueryStreamParameters>();
    pullQueryParameters.Should().BeOfType<PullQueryStreamParameters>();
  }
}
