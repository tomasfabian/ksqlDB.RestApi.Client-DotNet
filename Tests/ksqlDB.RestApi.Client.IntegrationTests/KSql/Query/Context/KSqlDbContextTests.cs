using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query.Context
{
  [TestClass]
  public class KSqlDbContextTests : Infrastructure.IntegrationTests
  {
    [TestMethod]
    public async Task AddAndSaveChangesAsync()
    {
      //Arrange
      var entity1 = new Movie { Id = 1, Title = "T1" };
      var entity2 = new Movie { Id = 2, Title = "T2" };

      //Act
      Context.Add(entity1);
      Context.Add(entity2);

      var response = await Context.SaveChangesAsync();

      //Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
  }
}