using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models.Sensors;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq.PullQueries
{
  [TestClass]
  public class PullQueryExtensionsTests
  {
    private SensorsPullQueryProvider pullQueryProvider;

    private KSqlDBContextOptions contextOptions;
    private KSqlDBContext context;

    [TestInitialize]
    public async Task TestInitialize()
    {
      contextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl);
      
      context = new KSqlDBContext(contextOptions);

      pullQueryProvider = new SensorsPullQueryProvider();

      await pullQueryProvider.ExecuteAsync();
    }

    [TestMethod]
    public async Task CreatePullQuery()
    {
      //Arrange
      string sensorId = "sensor-1";

      //Act
      var result = await context.CreatePullQuery<IoTSensorStats>(SensorsPullQueryProvider.MaterializedViewName)
        .Where(c => c.SensorId == sensorId)
        .GetAsync();
      
      //Assert
      result.Should().NotBeNull();
      result.SensorId.Should().Be(sensorId);
      result.WindowStart.Should().NotBe(null);
      result.WindowEnd.Should().NotBe(null);
    }

    [TestMethod]
    public async Task CreatePullQuery_WithBounds()
    {
      //Arrange
      string sensorId = "sensor-1";

      string windowStart = "2019-10-03T21:31:16";
      string windowEnd = "2225-10-03T21:31:16";

      //Act
      var result = await context.CreatePullQuery<IoTSensorStats>(SensorsPullQueryProvider.MaterializedViewName)
        .Where(c => c.SensorId == sensorId)
        .Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd)
        .GetAsync();
      
      //Assert
      result.Should().NotBeNull();
      result.SensorId.Should().Be(sensorId);
      result.WindowStart.Should().NotBe(null);
      result.WindowEnd.Should().NotBe(null);
    }

    [TestMethod]
    public async Task CreatePullQuery_FromPlainStringQuery()
    {
      //Arrange
      string sensorId = "sensor-1";
      string ksql = $"SELECT * FROM {SensorsPullQueryProvider.MaterializedViewName} WHERE SensorId = '{sensorId}';";

      //Act
      var result = await context.ExecutePullQuery<IoTSensorStats>(ksql);
      
      //Assert
      result.Should().NotBeNull();
      result.SensorId.Should().Be(sensorId);
    }
  }
}