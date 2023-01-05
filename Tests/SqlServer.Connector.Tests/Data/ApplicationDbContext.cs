using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConfigurationProvider = SqlServer.Connector.Tests.Config.ConfigurationProvider;

namespace SqlServer.Connector.Tests.Data;

public class ApplicationDbContext : DbContext
{
  public DbSet<IoTSensor> Sensors { get; set; } = null!;

  private readonly IConfiguration configuration = ConfigurationProvider.CreateConfiguration();
    
  string ConnectionString => configuration.GetConnectionString("DefaultConnection");

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);
      
    optionsBuilder.UseSqlServer(ConnectionString);
  }
}
