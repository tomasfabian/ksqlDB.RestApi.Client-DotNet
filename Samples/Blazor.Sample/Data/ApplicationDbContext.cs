using Blazor.Sample.Data.Sensors;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Sample.Data;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public DbSet<IoTSensor> Sensors { get; set; }
}