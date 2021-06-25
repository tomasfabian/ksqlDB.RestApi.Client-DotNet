using System.Threading.Tasks;

namespace Blazor.Sample.Providers
{
  public interface ISqlServerChangeDataCapture
  {
    Task CdcEnableTable(string tableName, string schemaName = "dbo");
    Task CdcDisableTableAsync(string tableName, string schemaName = "dbo");
    Task CdcEnableDbAsync();
    Task CdcDisableDbAsync();
  }
}