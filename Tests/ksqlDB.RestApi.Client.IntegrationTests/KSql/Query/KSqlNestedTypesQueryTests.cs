using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query
{
  [TestClass]
  public class KSqlNestedTypesQueryTests : KSqlNestedTypesTests
  {
    [ClassInitialize]
    public new static async Task ClassInitialize(TestContext context)
    {
      await InitializeDatabase();
    }

    [ClassCleanup]
    public new static async Task ClassCleanup()
    {
      await MoviesProvider.DropTablesAsync();
    }

    protected override IQbservable<Movie> MoviesStream => Context.CreateQuery<Movie>(MoviesTableName);
  }
}