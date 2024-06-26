using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Query;

public class KSqlNestedTypesQueryTests : KSqlNestedTypesTests
{
  [OneTimeSetUp]
  public new static async Task ClassInitialize()
  {
    await InitializeDatabase();
  }

  [OneTimeTearDown]
  public new static async Task ClassCleanup()
  {
    await MoviesProvider.DropTablesAsync();
  }

  protected override IQbservable<Movie> MoviesStream => Context.CreatePushQuery<Movie>(MoviesTableName);
}
