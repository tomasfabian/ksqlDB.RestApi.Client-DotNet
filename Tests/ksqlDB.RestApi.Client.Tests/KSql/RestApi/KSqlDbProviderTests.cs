using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Fakes.Logging;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi
{
  [TestClass]
  public class KSqlDbProviderTests : TestBase
  {  
    private TestableKSqlDbQueryStreamProvider ClassUnderTest { get; set; }

    private Mock<ILogger> LoggerMock { get; set; }
    
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      LoggerMock = MockingKernel.GetMock<ILogger>();

      ClassUnderTest = MockingKernel.Get<TestableKSqlDbQueryStreamProvider>();
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    [ExpectedException(typeof(KSqlQueryException))]
    public async Task Run_HttpStatusCodeBadRequest_ThrowsException()
    {
      //Arrange
      ClassUnderTest.ShouldThrowException = true;

      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = ClassUnderTest.Run<Tweet>(queryParameters);

      //Assert
      await foreach (var tweet in tweets)
      {
        tweet.Should().NotBeNull();
      }
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public async Task Run_HttpClientWasNotDisposed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      _ = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      ClassUnderTest.LastUsedHttpClient.IsDisposed.Should().BeFalse();
    }

    [TestMethod]
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
}