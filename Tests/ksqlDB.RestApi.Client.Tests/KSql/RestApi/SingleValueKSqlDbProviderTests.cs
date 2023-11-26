using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Ninject;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public class SingleValueKSqlDbProviderTests : TestBase
{
  [Test]
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

  [Test]
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
