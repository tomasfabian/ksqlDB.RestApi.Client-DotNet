using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDb.RestApi.Client.Tests.Fakes.Logging;
using ksqlDb.RestApi.Client.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Ninject;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public class KSqlDbProviderTests : TestBase
{  
  private TestableKSqlDbQueryStreamProvider ClassUnderTest { get; set; } = null!;

  private Mock<ILogger> LoggerMock { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    LoggerMock = MockingKernel.GetMock<ILogger>();

    ClassUnderTest = MockingKernel.Get<TestableKSqlDbQueryStreamProvider>();
  }

  [Test]
  public async Task Run_LogInformation()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    LoggerMock.VerifyLog(LogLevel.Information, Times.Once);
    LoggerMock.VerifyLog(LogLevel.Debug, () => Times.Exactly(3));
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_ReturnsTweets()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = ClassUnderTest.Run<Tweet>(queryParameters);

    //Assert
    var receivedTweets = new List<Tweet>();
    await foreach (var tweet in tweets)
    {
      tweet.Should().NotBeNull();
      receivedTweets.Add(tweet);
    }

    receivedTweets.Count.Should().Be(2);
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_StringFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.Message.Should().Be("Hello world");
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_BooleanFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.IsRobot.Should().BeTrue();
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_DoubleFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.Amount.Should().Be(0.00042);
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_DecimalFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.AccountBalance.Should().Be(9999999999999999.1234M);
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_BigintRowTimeFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.RowTime.Should().Be(1611327570881);
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_IntegerFieldWasParsed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    var tweet = tweets[0];

    tweet.Id.Should().Be(1);
  }

  [Test]
  public void Run_HttpStatusCodeBadRequest_ThrowsException()
  {
    //Arrange
    ClassUnderTest.ShouldThrowException = true;

    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = ClassUnderTest.Run<Tweet>(queryParameters);

    //Assert
    Assert.ThrowsAsync<KSqlQueryException>(() => tweets.ToListAsync().AsTask());
  }

  [Test]
  public async Task LogError()
  {
    //Arrange
    ClassUnderTest.Exception = new Exception("test");

    var queryParameters = new QueryStreamParameters();

    try
    {
      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
    }
    catch (Exception)
    {
      LoggerMock.VerifyLog(LogLevel.Error, Times.Once);
    }
  }

  [Test]
  public async Task Run_Disposed_NothingWasReceived()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();
    var cts = new CancellationTokenSource();

    //Act
    IAsyncEnumerable<Tweet> tweets = ClassUnderTest.Run<Tweet>(queryParameters, cts.Token);
    cts.Cancel();

    //Assert
    var receivedTweets = new List<Tweet>();

    await foreach (var tweet in tweets.WithCancellation(cts.Token))
    {
      receivedTweets.Add(tweet);
    }

    receivedTweets.Should().BeEmpty();
    cts.Dispose();
  }

  [Test]
  public async Task Run_HttpClientWasNotDisposed()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();
    ClassUnderTest.Options.DisposeHttpClient = false;

    //Act
    _ = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    ClassUnderTest.LastUsedHttpClient.IsDisposed.Should().BeFalse();
  }

  [Test]
  public async Task Run_DonNotDisposeHttpClient()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();
    ClassUnderTest.Options.DisposeHttpClient = true;

    //Act
    _ = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

    //Assert
    ClassUnderTest.LastUsedHttpClient.IsDisposed.Should().BeTrue();
  }
}
