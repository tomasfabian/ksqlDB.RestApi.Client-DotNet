using System;
using System.Globalization;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
{
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
      AccountBalance = 1.2M,
    };

    public static readonly Tweet Tweet2 = new()
    {
      Id = 2,
      Message = "Wall-e",
      IsRobot = false,
      Amount = 1, 
      AccountBalance = -5.6M,
    };

    public async Task<bool> CreateTweetsStream(string streamName, string topicName)
    {
      var ksql = $"CREATE OR REPLACE STREAM {streamName}(id INT, message VARCHAR, isRobot BOOLEAN, amount DOUBLE, accountBalance DECIMAL(16,4))\r\n  WITH (kafka_topic='{topicName}', value_format='json', partitions=1);";
      
      KSqlDbStatement ksqlDbStatement = new(ksql);

      var result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);

      return result.IsSuccess();
    }

    public string CreateInsertTweetStatement(Tweet tweet, string streamName)
    {
      var amount = tweet.Amount.ToString("E1", CultureInfo.InvariantCulture);

      string insert =
        $"INSERT INTO {streamName} (id, message, isRobot, amount, accountBalance) VALUES ({tweet.Id}, '{tweet.Message}', {tweet.IsRobot}, {amount}, {tweet.AccountBalance});";
      
      return insert;
    }

    public async Task<bool> InsertTweetAsync(Tweet tweet, string streamName)
    {
      var insert = CreateInsertTweetStatement(tweet, streamName);
      
      KSqlDbStatement ksqlDbStatement = new(insert);

      var result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);

      return result.IsSuccess();
    }
  }
}