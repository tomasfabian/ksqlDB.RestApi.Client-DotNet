using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi
{
  [TestClass]
  public class SingleValueKSqlDbProviderTests : TestBase
  {
    [TestMethod]
    public async Task Count_ParseSingleFields_IntegersAreConsumed()
    {
      //Arrange
      var provider = MockingKernel.Get<AggregationsKsqlDbQueryStreamProvider>();
      var queryParameters = new QueryStreamParameters();

      //Act
      var counts = provider.Run<int>(queryParameters);     
      
      //Assert
      (await counts.ToListAsync()).Count.Should().Be(2);
    }

    [TestMethod]
    public async Task NewCount_ParseSingleFields_IntegersAreConsumed()
    {
      //Arrange

      //Act
      var counts = Run(new { Count = 1 });     
      
      //Assert
      (await counts.ToListAsync()).Count.Should().Be(2);
    }
    
    IAsyncEnumerable<T> Run<T>(T anonymousType) {
      var provider = MockingKernel.Get<AggregationsKsqlDbQueryStreamProvider>();
      var queryParameters = new QueryStreamParameters();

      var asyncEnumerable = provider.Run<T>(queryParameters);

      return asyncEnumerable;
    }
  }
}