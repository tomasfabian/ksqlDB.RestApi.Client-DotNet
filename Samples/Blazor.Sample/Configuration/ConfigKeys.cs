using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Sample.Configuration
{
  public static class ConfigKeys
  {
    public static string KSqlDb_Url => "ksqlDb:Url";
    public static string Kafka_BootstrapServers => "Kafka:BootstrapServers";
  }
}
