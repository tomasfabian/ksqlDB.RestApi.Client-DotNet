using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Blazor.Sample.Data;

public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder()        
      .Build();
      
    var connectionString = configuration["ConnectionStrings:DefaultConnection"];

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(connectionString);

    return new ApplicationDbContext(optionsBuilder.Options);
  }
}