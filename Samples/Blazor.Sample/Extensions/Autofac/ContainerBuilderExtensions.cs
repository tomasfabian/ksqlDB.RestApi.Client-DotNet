using Autofac;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka.Consumers;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using InsideOut.Consumer;
using InsideOut.Producer;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Connect;

namespace Blazor.Sample.Extensions.Autofac;

public static class ContainerBuilderExtensions
{
  public static void OnRegisterTypes(this ContainerBuilder containerBuilder, IConfiguration configuration)
  {
    string connectionString = configuration["ConnectionStrings:DefaultConnection"];

    containerBuilder.RegisterType<CdcClient>()
      .As<ISqlServerCdcClient>()
      .SingleInstance()
      .WithParameter(nameof(connectionString), connectionString);

    Uri ksqlDbUrl = new Uri(configuration["ksqlDb:Url"]);

    containerBuilder.RegisterType<KsqlDbConnect>()
      .As<IKsqlDbConnect>()
      .SingleInstance()
      .WithParameter(nameof(ksqlDbUrl), ksqlDbUrl);

    string bootstrapServers = configuration["Kafka:BootstrapServers"];

    RegisterConsumers(containerBuilder, bootstrapServers);

    RegisterProducers(containerBuilder, bootstrapServers);
  }

  public static void RegisterProducers(this ContainerBuilder containerBuilder, string bootstrapServers)
  {
    var producerConfig = new ProducerConfig
    {
      BootstrapServers = bootstrapServers,
      Acks = Acks.Leader
    };

    containerBuilder.RegisterInstance(producerConfig);

    containerBuilder.RegisterType<KafkaProducer<int, IoTSensor>>()
      .As<IKafkaProducer<int, IoTSensor>>()
      .WithParameter("topicName", TopicNames.IotSensors)
      .WithParameter(nameof(producerConfig), producerConfig);
  }

  public static void RegisterConsumers(this ContainerBuilder containerBuilder, string bootstrapServers)
  {
    var consumerConfig = new ConsumerConfig
    {
      BootstrapServers = bootstrapServers,
      ClientId = "Client01" + "_consumer",
      GroupId = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
      AutoOffsetReset = AutoOffsetReset.Latest,
      PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
    };

    containerBuilder.RegisterInstance(consumerConfig);

    containerBuilder.RegisterType<SensorsStreamConsumer>()
      .As<IKafkaConsumer<string, SensorsStream>>()
      .WithParameter(nameof(consumerConfig), consumerConfig);

    consumerConfig.ClientId = "Client02" + "_consumer";
    consumerConfig.GroupId = $"{nameof(IoTSensorStats)}";

    containerBuilder.RegisterType<SensorsTableConsumer>()
      .As<IKafkaConsumer<string, IoTSensorStats>>()
      .WithParameter(nameof(consumerConfig), consumerConfig);
  }
}