using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Blazor.Sample.Kafka.Consumers;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.InsideOut.Producer;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Sample
{
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

      var connectionString = Configuration["ConnectionStrings:DefaultConnection"];

      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    }

    private ContainerBuilder ContainerBuilder { get; set; }

    public void ConfigureContainer(ContainerBuilder builder)
    {
      ContainerBuilder = builder;

      OnRegisterTypes(builder);
    }

    protected virtual void OnRegisterTypes(ContainerBuilder containerBuilder)
    {
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
        AutoOffsetReset = AutoOffsetReset.Latest
      };

      containerBuilder.RegisterInstance(consumerConfig);

      containerBuilder.RegisterType<SensorsStreamConsumer>()
        .As<IKafkaConsumer<string, SensorsStream>>()
        .SingleInstance()
        .WithParameter(nameof(consumerConfig), consumerConfig);

      consumerConfig.ClientId = "Client02" + "_consumer";
      consumerConfig.GroupId = $"{nameof(IoTSensorStats)}";

      containerBuilder.RegisterType<SensorsTableConsumer>()
        .As<IKafkaConsumer<string, IoTSensorStats>>()
        .SingleInstance()
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
}