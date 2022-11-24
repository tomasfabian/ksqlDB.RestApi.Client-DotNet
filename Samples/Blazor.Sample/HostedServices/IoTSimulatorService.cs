using System.Reactive.Linq;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using InsideOut.Producer;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace Blazor.Sample.HostedServices;

public class IoTSimulatorService : IHostedService
{
  private readonly IKafkaProducer<int, IoTSensor> kafkaProducer;
  private readonly IConfiguration configuration;

  public IoTSimulatorService(IKafkaProducer<int, IoTSensor> kafkaProducer, IConfiguration configuration)
  {
    this.kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
    this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
  }

  private readonly Random randomValue = new(10);
  private readonly Random randomKey = new(1);

  private IDisposable timerSubscription;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await TryCreateStreamAsync(cancellationToken);

    timerSubscription =
      Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
        .Subscribe(async _ =>
        {
          int key = randomKey.Next(1, 10);
          int value = randomValue.Next(1, 100);

          var sensor = new IoTSensor
          {
            SensorId = $"Sensor-{key}", 
            Value = value
          };

          var deliveryResult = await kafkaProducer.ProduceMessageAsync(key, sensor, cancellationToken);
        });
  }
    
  private string KsqlDbUrl => configuration[ConfigKeys.KSqlDb_Url];

  private async Task<HttpResponseMessage> TryCreateStreamAsync(CancellationToken cancellationToken)
  {
    EntityCreationMetadata metadata = new()
    {
      KafkaTopic = TopicNames.IotSensors,
      Partitions = 1,
      Replicas = 1
    };

    var httpClient = new HttpClient
    {
      BaseAddress = new Uri(KsqlDbUrl)
    };

    var httpClientFactory = new HttpClientFactory(httpClient);
    var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

    var httpResponseMessage = await restApiClient.CreateStreamAsync<IoTSensor>(metadata, ifNotExists: true, cancellationToken);

    return httpResponseMessage;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    using (timerSubscription)
    {
    }

    return Task.CompletedTask;
  }
}
