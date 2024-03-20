using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using Statements.Model;
using Statements.Model.Events;

const string ksqlDbUrl = @"http://localhost:8088";

var servicesCollection = new ServiceCollection();
servicesCollection.ConfigureKSqlDb(ksqlDbUrl);

var serviceProvider = servicesCollection.BuildServiceProvider();
IKSqlDbRestApiClient ksqlDbRestApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

await InsertAComplexTypeAsync(ksqlDbRestApiClient);

Console.WriteLine("Press any key to stop the subscription");

Console.ReadKey();

#pragma warning disable CS8321 // Local function is declared but never used

static async Task InsertAComplexTypeAsync(IKSqlDbRestApiClient restApiClient)
{
  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop type {nameof(EventCategory)};
Drop table {nameof(Event)};
"));

  httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
  httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata("Events") { Partitions = 1 });

  var eventCategory = new EventCategory
  {
    Name = "Planet Earth"
  };

  var testEvent = new Event
  {
    Id = 42,
    Places = new[] { "Place1", "Place2", "Place3" },
  };

  httpResponseMessage = await restApiClient.InsertIntoAsync(testEvent);
}

static async Task AssertsAsync(IKSqlDbRestApiClient restApiClient)
{
  var assertSchemaOptions = new AssertSchemaOptions("Kafka-key3", 1)
  {
    Timeout = Duration.OfSeconds(3)
  };

  var assertSchemaResponse = await restApiClient.AssertSchemaNotExistsAsync(assertSchemaOptions);
  assertSchemaResponse = await restApiClient.AssertSchemaExistsAsync(assertSchemaOptions);

  Console.WriteLine(assertSchemaResponse[0].Subject);

  var topicProperties = new Dictionary<string, string>
  {
    { "replicas", "1" },
    { "partitions", "1" },
  };

  var options = new AssertTopicOptions("tweetsByTitle")
  {
    Properties = topicProperties,
    Timeout = Duration.OfSeconds(3)
  };

  var assertTopicResponse = await restApiClient.AssertTopicNotExistsAsync(options);
  assertTopicResponse = await restApiClient.AssertTopicExistsAsync(options);

  Console.WriteLine(assertTopicResponse[0].TopicName);
}

static async Task CreateTypesAsync(IKSqlDbRestApiClient restApiClient)
{
  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
Drop type Person;
Drop type Address;
"));

  //Act
  httpResponseMessage = await restApiClient.CreateTypeAsync<Address>();
  httpResponseMessage = await restApiClient.CreateTypeAsync<Person>();
}

static async Task CreateTypeWithSessionVariableAsync(IKSqlDbRestApiClient restApiClient)
{
  var statement = new KSqlDbStatement("CREATE TYPE ${typeName} AS STRUCT<name VARCHAR, address ADDRESS>;")
  {
    SessionVariables = { { "typeName", "FromSessionValue" } }
  };

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(statement);
}

static async Task CreateStreamAsync(IKSqlDbRestApiClient restApiClient)
{
  EntityCreationMetadata metadata = new(kafkaTopic: nameof(Event))
  {
    Partitions = 1,
    Replicas = 1
  };

  var httpResponseMessage = await restApiClient.CreateStreamAsync<Event>(metadata, ifNotExists: true);

  //OR
  //httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<MovieNullableFields>(metadata);

  httpResponseMessage = await restApiClient.CreateSourceStreamAsync<Event>(metadata, ifNotExists: true);

  string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
}

static async Task GetKsqlDbInformationAsync(IKSqlDbRestApiClient restApiProvider)
{
  Console.WriteLine($"{Environment.NewLine}Available topics:");
  var topicsResponses = await restApiProvider.GetTopicsAsync();
  Console.WriteLine(string.Join(',', topicsResponses[0].Topics.Select(c => c.Name)));

  TopicsResponse[] allTopicsResponses = await restApiProvider.GetAllTopicsAsync();
  TopicsExtendedResponse[] topicsExtendedResponses = await restApiProvider.GetTopicsExtendedAsync();
  var allTopicsExtendedResponses = await restApiProvider.GetAllTopicsExtendedAsync();

  Console.WriteLine($"{Environment.NewLine}Available tables:");
  var tablesResponse = await restApiProvider.GetTablesAsync();
  Console.WriteLine(string.Join(',', tablesResponse[0].Tables.Select(c => c.Name)));

  Console.WriteLine($"{Environment.NewLine}Available streams:");
  var streamsResponse = await restApiProvider.GetStreamsAsync();
  Console.WriteLine(string.Join(',', streamsResponse[0].Streams.Select(c => c.Name)));

  Console.WriteLine($"{Environment.NewLine}Available connectors:");
  var connectorsResponse = await restApiProvider.GetConnectorsAsync();
  Console.WriteLine(string.Join(',', connectorsResponse[0].Connectors.Select(c => c.Name)));
}

static async Task TerminatePushQueryAsync(IKSqlDBContext context, IKSqlDbRestApiClient restApiClient)
{
  var subscription = await context.CreateQueryStream<Event>()
    .SubscribeOn(ThreadPoolScheduler.Instance)
    .SubscribeAsync(onNext: _ => { }, onError: e => { }, onCompleted: () => { });

  var response = await restApiClient.TerminatePushQueryAsync(subscription.QueryId);
}

static async Task TerminatePersistentQueryAsync(IKSqlDbRestApiClient restApiClient)
{
  string topicName = "moviesByTitle";

  var queries = await restApiClient.GetQueriesAsync();

  var query = queries.SelectMany(c => c.Queries).FirstOrDefault(c => c.SinkKafkaTopics.Contains(topicName));

  if (query == null)
    return;

  await TerminatePersistentQueryByIdAsync(restApiClient, query.Id);
}

static async Task TerminatePersistentQueryByIdAsync(IKSqlDbRestApiClient restApiClient, string queryId)
{
  var response = await restApiClient.PausePersistentQueryAsync(queryId);
  response = await restApiClient.ResumePersistentQueryAsync(queryId);
  response = await restApiClient.TerminatePersistentQueryAsync(queryId);
}

static async Task CreateConnectorsAsync(IKSqlDbRestApiClient restApiClient)
{
  string sourceConnectorName = "mock-source-connector";
  string sinkConnectorName = "mock-sink-connector";

  var sourceConnectorConfig = new Dictionary<string, string>
  {
    {"connector.class", "org.apache.kafka.connect.tools.MockSourceConnector"}
  };

  var httpResponseMessage = await restApiClient.CreateSourceConnectorAsync(sourceConnectorConfig, sourceConnectorName);

  var sinkConnectorConfig = new Dictionary<string, string> {
    { "connector.class", "org.apache.kafka.connect.tools.MockSinkConnector" },
    { "topics.regex", "mock-sink*"},
  };

  httpResponseMessage = await restApiClient.CreateSinkConnectorAsync(sinkConnectorConfig, sinkConnectorName);

  httpResponseMessage = await restApiClient.DropConnectorAsync($"`{sinkConnectorName}`");
}

#pragma warning restore CS8321 // Local function is declared but never used
