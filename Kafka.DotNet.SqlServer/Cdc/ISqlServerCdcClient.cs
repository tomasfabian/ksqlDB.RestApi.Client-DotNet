using System.Threading.Tasks;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public interface ISqlServerCdcClient : ICdcClient
  {
    Task CdcEnableTableAsync(string tableName, string schemaName = "dbo");
    Task CdcEnableTableAsync(CdcEnableTable cdcEnableTable);

    Task CdcDisableTableAsync(string tableName, string schemaName = "dbo", string captureInstance = "all");
  }
}