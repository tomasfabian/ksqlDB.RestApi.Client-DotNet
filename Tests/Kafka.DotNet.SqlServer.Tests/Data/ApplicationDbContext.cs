using Microsoft.EntityFrameworkCore;

namespace Kafka.DotNet.SqlServer.Tests.Data
{
  public class ApplicationDbContext : DbContext
  {
    public DbSet<IoTSensor> Sensors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      base.OnConfiguring(optionsBuilder);
      
      string connectionString =
        "Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = TestSensors;MultipleActiveResultSets=true";

      optionsBuilder.UseSqlServer(connectionString);
    }
  }
}