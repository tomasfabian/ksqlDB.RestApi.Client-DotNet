using System.Threading.Tasks;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public interface ISqlServerCdcClient
  {
    Task CdcEnableTable(string tableName, string schemaName = "dbo");
    Task CdcDisableTableAsync(string tableName, string schemaName = "dbo");
    Task CdcEnableDbAsync();
    Task CdcDisableDbAsync();
  }
}