using System.Threading.Tasks;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public interface ICdcClient
  {
    Task EnableAsync(string tableName, string schemaName = "dbo");
    Task DisableAsync(string tableName, string schemaName = "dbo");
  }
}