using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

[TestClass]
public class MapValuesKSqlDbProviderTests : TestBase
{
  [TestMethod]
  public async Task MapsAssociativeDataType()
  {
    //Arrange

    //Act
    var results = Run(new { KSQL_COL_0 = new Dictionary<string, int>() });     

    //Assert
    var resultList = await results.ToListAsync(); 
      
    resultList.Count.Should().Be(2);

    resultList[0].KSQL_COL_0.Count.Should().Be(2);
    resultList[0].KSQL_COL_0["a"].Should().Be(1);
  }
    
  IAsyncEnumerable<T> Run<T>(T anonymousType) {
    var provider = MockingKernel.Get<MapResultsKsqlDbQueryStreamProvider>();
    var queryParameters = new QueryStreamParameters();

    var asyncEnumerable = provider.Run<T>(queryParameters);

    return asyncEnumerable;
  }
}