using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query.Context
{
  [TestClass]
  public class KSqlDbContextTests : Infrastructure.IntegrationTests
  {
    private const string EntityName = "movies_test112";

    [TestMethod]
    public async Task AddAndSaveChangesAsync()
    {
      //Arrange
      var config = new InsertProperties { EntityName = EntityName, ShouldPluralizeEntityName = false };
      var entity1 = new Movie { Id = 1, Title = "T1" };
      var entity2 = new Movie { Id = 2, Title = "T2" };

      //Act
      Context.Add(entity1, config);
      Context.Add(entity2, config);

      var response = await Context.SaveChangesAsync();
      var c = await response.Content.ReadAsStringAsync();

      //Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task AddDbContextFactory_ContextFactoryCreate_SaveChangesAsync()
    {
      //Arrange
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(KSqlDbRestApiProvider.KsqlDbUrl), ServiceLifetime.Transient);
      serviceCollection.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

      var contextFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

      var config = new InsertProperties { EntityName = EntityName, ShouldPluralizeEntityName = false };
      var entity1 = new Movie { Id = 3, Title = "T3" };
      var entity2 = new Movie { Id = 4, Title = "T4" };

      //Act
      await using var context = contextFactory.Create();

      context.Add(entity1, config);
      context.Add(entity2, config);

      var response = await context.SaveChangesAsync();

      //Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
  }
}