using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace ksqlDB.Api.Client.Tests.Fakes.Logging;

internal static class LoggerMockExtensions
{
  public static void VerifyLog(this Mock<ILogger> mockedLogger, LogLevel logLevel, Func<Times> times)
  {
    mockedLogger.Verify(c => c.Log(
      It.Is<LogLevel>(l => l == logLevel),
      It.IsAny<EventId>(),
      It.Is<It.IsAnyType>((v, t) => true),
      It.IsAny<Exception>(),
      It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
  }
}