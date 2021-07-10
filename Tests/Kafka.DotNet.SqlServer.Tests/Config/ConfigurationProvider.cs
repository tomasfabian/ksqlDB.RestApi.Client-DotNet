using System;
using Microsoft.Extensions.Configuration;

namespace Kafka.DotNet.SqlServer.Tests.Config
{
  public static class ConfigurationProvider
  {
    public static IConfiguration CreateConfiguration()
    {
      return new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", false)
        .Build();
    }
  }
}