using System.Globalization;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class TweetsProvider
{
  private readonly KSqlDbRestApiProvider restApiProvider;

  public TweetsProvider(KSqlDbRestApiProvider restApiProvider)
  {
    this.restApiProvider = restApiProvider ?? throw new ArgumentNullException(nameof(restApiProvider));
  }
    
  public static readonly Tweet Tweet1 = new()
  {
    Id = 1,
    Message = "Hello world",
    IsRobot = true,
    Amount = 0.00042, 
    AccountBalance = 533333333421.6332M,
  };

  public static readonly Tweet Tweet2 = new()
  {
    Id = 2,
    Message = "Wall-e",
    IsRobot = false,
    Amount = 1, 
    AccountBalance = -5.6M
  };

  public async Task<bool> CreateTweetsStream(string streamName, string topicName)
  {
    var ksql = $"CREATE OR REPLACE STREAM {streamName}(id INT, message VARCHAR, isRobot BOOLEAN, amount DOUBLE, accountBalance DECIMAL(16,4))\r\n  WITH (kafka_topic='{topicName}', value_format='json', partitions=1);";
      
    KSqlDbStatement ksqlDbStatement = new(ksql);

    var result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);

    return result.IsSuccess();
  }

  public async Task<bool> InsertTweetAsync(Tweet tweet, string streamName)
  {
    var insertProperties = new InsertProperties()
    {
      EntityName = streamName,
      ShouldPluralizeEntityName = false,
      FormatDoubleValue = value => value.ToString("E1", CultureInfo.InvariantCulture),
      FormatDecimalValue = value => value.ToString(CultureInfo.CreateSpecificCulture("en-GB"))
    };

    var result = await restApiProvider.InsertIntoAsync(tweet, insertProperties); 

    return result.IsSuccess();
  }
}