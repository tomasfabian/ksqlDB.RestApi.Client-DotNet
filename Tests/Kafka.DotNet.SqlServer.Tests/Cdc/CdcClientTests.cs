using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.SqlServer.Cdc;
using Kafka.DotNet.SqlServer.Tests.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.SqlServer.Tests.Cdc
{
  [TestClass]
  public class CdcClientTests : TestBase<CdcClient>
  {
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      var dbContext = new ApplicationDbContext();

      await dbContext.Database.EnsureDeletedAsync();
      
      await dbContext.Database.MigrateAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      var dbContext = new ApplicationDbContext();

      await dbContext.Database.EnsureDeletedAsync();
    }
    
    string ConnectionString =>
      "Server=127.0.0.1,1433;User Id = SA;Password=<YourNe" +
      "wStrong@Passw0rd>;Initial Catalog = TestSensors;MultipleActiveResultSets=true";

    /// <summary>
    /// Has SQL Server database enabled Change Data Capture (CDC) 
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public Task<bool> IsCdcDbEnabledAsync(string databaseName)
    {
      return ExecuteScalarAsync($"SELECT COUNT(*) FROM sys.databases\r\nWHERE is_cdc_enabled = 1 AND name = '{databaseName}'");
    }    

    /// <summary>
    /// Has table Change Data Capture (CDC) enabled on a SQL Server database
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public Task<bool> IsCdcTableEnabledAsync(string tableName, string schemaName = "dbo")
    {
      string sql = $@"SELECT COUNT(*)
FROM sys.tables tb
INNER JOIN sys.schemas s on s.schema_id = tb.schema_id
WHERE tb.is_tracked_by_cdc = 1 AND tb.name = '{tableName}'
AND s.name = '{schemaName}'";

      return ExecuteScalarAsync(sql);
    } 
    
    private async Task<bool> ExecuteScalarAsync(string cmdText)
    {
      bool result;

      using (var sqlConnection = new SqlConnection(ConnectionString))
      {
        await sqlConnection.OpenAsync().ConfigureAwait(false);

        var sqlCommand = new SqlCommand(cmdText, sqlConnection);

        var response = await sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);

        result = (int)response > 0;

        await sqlConnection.CloseAsync().ConfigureAwait(false);
      }

      return result;
    }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new CdcClient(ConnectionString);
    }

    [TestMethod]
    public async Task CdcEnableDbAsync()
    {
      //Arrange
      string databaseName = "TestSensors";

      //Act
      await ClassUnderTest.CdcEnableDbAsync();
      
      var isEnabled = await IsCdcDbEnabledAsync(databaseName);

      //Assert
      isEnabled.Should().Be(true);
    }

    [TestMethod]
    public async Task CdcEnableTableAsync()
    {
      //Arrange
      string tableName = "Sensors";

      //Act
      await ClassUnderTest.CdcEnableTableAsync(tableName);

      var isEnabled = await IsCdcTableEnabledAsync(tableName);

      //Assert
      isEnabled.Should().Be(true);
    }

    [TestMethod]
    public async Task CdcDisableTableAsync()
    {
      //Arrange
      string tableName = "Sensors";

      //Act
      await ClassUnderTest.CdcDisableTableAsync(tableName);

      var isEnabled = await IsCdcTableEnabledAsync(tableName);

      //Assert
      isEnabled.Should().Be(false);
    }

    [TestMethod]
    public async Task CdcDisableDbAsync()
    {
      //Arrange
      string databaseName = "TestSensors";

      //Act
      await ClassUnderTest.CdcDisableDbAsync();

      //Assert

      var isEnabled = await IsCdcDbEnabledAsync(databaseName);

      //Assert
      isEnabled.Should().Be(false);
    }
  }
}