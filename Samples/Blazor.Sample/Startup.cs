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
using Blazor.Sample.KafkaConsumers;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;

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

    protected virtual void OnRegisterTypes(ContainerBuilder builder)
    {
      string bootstrapServers = "localhost:29092";

      RegisterConsumers(builder, bootstrapServers);
    }

    private static void RegisterConsumers(ContainerBuilder containerBuilder, string bootstrapServers)
    {
      var consumerConfig = new ConsumerConfig
      {
        BootstrapServers = bootstrapServers,
        GroupId = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
      };

      containerBuilder.RegisterInstance(consumerConfig);

      containerBuilder.RegisterType<ItemsKafkaConsumer>()
        .As<IKafkaConsumer<int, Item>>()
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