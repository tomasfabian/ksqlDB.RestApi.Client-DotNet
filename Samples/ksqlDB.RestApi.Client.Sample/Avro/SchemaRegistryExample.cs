using System;
using System.Threading.Tasks;
using Confluent.SchemaRegistry;
using ksqlDB.Api.Client.Samples.Http;
using Sensors;
using ksqlDB.Api.Client.Samples.Providers;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.Api.Client.Samples.Avro
{
  public class SchemaRegistryExample
  {
    public async Task AvroExampleAsync(string ksqlDbUrl)
    {
      var schemaRegistryConfig = new SchemaRegistryConfig
      {
        Url = "http://localhost:8081"
      };
      
      using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

      var schema = IoTSensor._SCHEMA.ToString();

      var subject = "IoTSensors-value";

      var registrationResult = await schemaRegistry.RegisterSchemaAsync(subject, schema);

      //http://localhost:8081/subjects/IoTSensors-value/versions/latest/schema
      var latestSchema = await schemaRegistry.GetLatestSchemaAsync(subject);

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
      var restApiClient = new KSqlDbRestApiProvider(httpClientFactory);

      EntityCreationMetadata metadata = new()
      {
        EntityName = "avroSensors",
        KafkaTopic = "IoTSensors",
        ValueFormat = SerializationFormats.Avro,
        Partitions = 1,
        Replicas = 1
      };
      
      var httpResponseMessage = await restApiClient.CreateStreamAsync<IoTSensor>(metadata, ifNotExists: false)
        .ConfigureAwait(false);

      var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync();

      Console.WriteLine(httpResponse);

      // Stream Name         | Kafka Topic | Key Format | Value Format | Windowed
      //   ------------------------------------------------------------------------------------------------------
      // AVROSENSORS | IoTSensors | KAFKA | AVRO | false
    }
  }
}