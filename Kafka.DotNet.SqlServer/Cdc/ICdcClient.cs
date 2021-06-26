using System.Threading.Tasks;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public interface ICdcClient
  {    
    Task CdcEnableDbAsync();
    Task CdcDisableDbAsync();
  }
}