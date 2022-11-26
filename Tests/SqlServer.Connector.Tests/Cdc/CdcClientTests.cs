using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Tests.Data;
using UnitTests;
using ConfigurationProvider = SqlServer.Connector.Tests.Config.ConfigurationProvider;

namespace SqlServer.Connector.Tests.Cdc;

[TestClass]
[TestCategory("Integration")]
public class CdcClientTests : TestBase<CdcClient>
{
  [ClassInitialize]
  public static async Task ClassInitialize(TestContext context)
  {
    var dbContext = new ApplicationDbContext();

    await dbContext.Database.EnsureDeletedAsync();
      
    await dbContext.Database.MigrateAsync();
  }

  private static readonly IConfiguration Configuration = ConfigurationProvider.CreateConfiguration();

  [ClassCleanup]
  public static async Task ClassCleanup()
  {
    var dbContext = new ApplicationDbContext();

    await dbContext.Database.EnsureDeletedAsync();
  }
    
  static string ConnectionString => Configuration.GetConnectionString("DefaultConnection");

  readonly SqlConnectionStringBuilder connectionStringBuilder = new(ConnectionString);

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
    string databaseName = connectionStringBuilder.InitialCatalog;

    //Act
    await ClassUnderTest.CdcEnableDbAsync();
      
    var isEnabled = await ClassUnderTest.IsCdcDbEnabledAsync(databaseName);

    //Assert
    isEnabled.Should().Be(true);
  }

  [TestMethod]
  public async Task CdcEnableTableAsync()
  {
    //Arrange

    //Act
    await ClassUnderTest.CdcEnableTableAsync(tableName);

    var isEnabled = await ClassUnderTest.IsCdcTableEnabledAsync(tableName);

    //Assert
    isEnabled.Should().Be(true);
  }
    
  string tableName = "Sensors";

  [TestMethod]
  public async Task CdcEnableTableAsync_CdcEnableTableInput()
  {
    //Arrange
    string captureInstance = $"dbo_{tableName}_v2";

    var cdcEnableTable = new CdcEnableTable(tableName)
    {
      CaptureInstance = captureInstance
    };

    //Act
    await ClassUnderTest.CdcEnableTableAsync(cdcEnableTable);

    var isEnabled = await ClassUnderTest.IsCdcTableEnabledAsync(tableName, captureInstance: captureInstance);

    //Assert
    isEnabled.Should().Be(true);
  }
    
  [TestMethod]
  public async Task CdcDisableTableAsync_CaptureInstance()
  {
    //Arrange
    string captureInstance = $"dbo_{tableName}_v2";

    //Act
    await ClassUnderTest.CdcDisableTableAsync(tableName, captureInstance: captureInstance);

    var isEnabled = await ClassUnderTest.IsCdcTableEnabledAsync(tableName, captureInstance: captureInstance);

    //Assert
    isEnabled.Should().Be(false);
  }

  [TestMethod]
  public async Task CdcDisableTableAsync()
  {
    //Arrange

    //Act
    await ClassUnderTest.CdcDisableTableAsync(tableName);

    var isEnabled = await ClassUnderTest.IsCdcTableEnabledAsync(tableName);

    //Assert
    isEnabled.Should().Be(false);
  }

  [TestMethod]
  public async Task CdcDisableDbAsync()
  {
    //Arrange
    string databaseName = connectionStringBuilder.InitialCatalog;

    //Act
    await ClassUnderTest.CdcDisableDbAsync();

    //Assert

    var isEnabled = await ClassUnderTest.IsCdcDbEnabledAsync(databaseName);

    //Assert
    isEnabled.Should().Be(false);
  }
}
