using Confluent.SchemaRegistry;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Sensors;

var schemaRegistryConfig = new SchemaRegistryConfig
{
  Url = @"http://localhost:8081"
};

using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

var subject = "IoTSensors-value";
var schema = IoTSensor._SCHEMA.ToString();

var registrationResult = await schemaRegistry.RegisterSchemaAsync(subject, schema);

//http://localhost:8081/subjects/IoTSensors-value/versions/latest/schema
var latestSchema = await schemaRegistry.GetLatestSchemaAsync(subject);

Console.WriteLine($"Latest schema: {latestSchema}");

var ksqlDbUrl = @"http:\\localhost:8088";

var httpClient = new HttpClient()
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new ksqlDB.RestApi.Client.KSql.RestApi.Http.HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

EntityCreationMetadata metadata = new()
{
  EntityName = "avroSensors",
  KafkaTopic = "IoTSensors",
  ValueFormat = SerializationFormats.Avro,
  Partitions = 1,
  Replicas = 1
};

var httpResponseMessage = await restApiClient.CreateStreamAsync<IoTSensor>(metadata, false)
  .ConfigureAwait(false);

var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync();

Console.WriteLine(httpResponse);
