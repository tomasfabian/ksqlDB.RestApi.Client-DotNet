using System.Threading.Tasks;

namespace Blazor.Sample.Providers
{
  public interface ISqlServerChangeDataCaptureProvider
  {
    Task EnableAsync(string tableName, string schemaName = "dbo");
    Task DisableAsync(string tableName, string schemaName = "dbo");
  }
}