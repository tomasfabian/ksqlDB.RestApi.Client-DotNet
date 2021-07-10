using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConfigurationProvider = Kafka.DotNet.SqlServer.Tests.Config.ConfigurationProvider;

namespace Kafka.DotNet.SqlServer.Tests.Data
{
  public class ApplicationDbContext : DbContext
  {
    public DbSet<IoTSensor> Sensors { get; set; }
    
    private readonly IConfiguration configuration = ConfigurationProvider.CreateConfiguration();
    
    string ConnectionString => configuration.GetConnectionString("DefaultConnection");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      base.OnConfiguring(optionsBuilder);
      
      optionsBuilder.UseSqlServer(ConnectionString);
    }
  }
}