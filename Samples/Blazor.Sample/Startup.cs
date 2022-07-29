using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Blazor.Sample.Kafka.Consumers;
using Confluent.Kafka;
using InsideOut.Consumer;
using InsideOut.Producer;
using Microsoft.EntityFrameworkCore;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Connect;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace Blazor.Sample;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddRazorPages();
    services.AddServerSideBlazor();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(options =>
      {
        string ksqlDbUrl = Configuration["ksqlDb:Url"];

        var setupParameters = options.UseKSqlDb(ksqlDbUrl);

      }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

    ConfigureEntityFramework(services);
  }

  private void ConfigureEntityFramework(IServiceCollection services)
  {
    var connectionString = Configuration.GetConnectionString("DefaultConnection");

    services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseSqlServer(connectionString); });

    services.AddScoped(p =>
      p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
        .CreateDbContext());
  }

  private ContainerBuilder ContainerBuilder { get; set; }

  public void ConfigureContainer(ContainerBuilder builder)
  {
    ContainerBuilder = builder;

    OnRegisterTypes(builder);
  }

  protected virtual void OnRegisterTypes(ContainerBuilder containerBuilder)
  {
    string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

    containerBuilder.RegisterType<CdcClient>()
      .As<ISqlServerCdcClient>()
      .SingleInstance()
      .WithParameter(nameof(connectionString), connectionString);

    Uri ksqlDbUrl = new Uri(Configuration["ksqlDb:Url"]);

    containerBuilder.RegisterType<KsqlDbConnect>()
      .As<IKsqlDbConnect>()
      .SingleInstance()
      .WithParameter(nameof(ksqlDbUrl), ksqlDbUrl);

    string bootstrapServers = Configuration["Kafka:BootstrapServers"];

    RegisterConsumers(containerBuilder, bootstrapServers);

    RegisterProducers(containerBuilder, bootstrapServers);
  }

  private static void RegisterProducers(ContainerBuilder containerBuilder, string bootstrapServers)
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

  private static void RegisterConsumers(ContainerBuilder containerBuilder, string bootstrapServers)
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

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapBlazorHub();
      endpoints.MapFallbackToPage("/_Host");
    });
  }
}