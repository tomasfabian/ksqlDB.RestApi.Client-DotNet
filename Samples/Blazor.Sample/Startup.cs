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
using Blazor.Sample.Kafka.Consumers;
using Blazor.Sample.Kafka.Producers;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.InsideOut.Producer;

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
    }

    private ContainerBuilder ContainerBuilder { get; set; }

    public void ConfigureContainer(ContainerBuilder builder)
    {
      ContainerBuilder = builder;

      OnRegisterTypes(builder);
    }

    protected virtual void OnRegisterTypes(ContainerBuilder containerBuilder)
    {
      string bootstrapServers = "broker01:9092";

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

      containerBuilder.RegisterType<ItemsKafkaProducer>()
        .As<IKafkaProducer<int, Item>>()
        .WithParameter(nameof(producerConfig), producerConfig);
    }

    private static void RegisterConsumers(ContainerBuilder containerBuilder, string bootstrapServers)
    {
      var consumerConfig = new ConsumerConfig
      {
        BootstrapServers = bootstrapServers,
        GroupId = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
        AutoOffsetReset = AutoOffsetReset.Latest
      };

      containerBuilder.RegisterInstance(consumerConfig);

      containerBuilder.RegisterType<ItemsKafkaConsumer>()
        .As<IKafkaConsumer<int, ItemStream>>()
        .SingleInstance()
        .WithParameter(nameof(consumerConfig), consumerConfig);

      containerBuilder.RegisterType<ItemsTableKafkaConsumer>()
        .As<IKafkaConsumer<int, ItemTable>>()
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