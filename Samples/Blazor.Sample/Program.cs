using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.HostedServices;
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

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Configuration.AddEnvironmentVariables();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHostedService<IoTSimulatorService>();

ConfigureServices(builder.Services, builder.Configuration);

builder.Host.ConfigureContainer<ContainerBuilder>(c =>
{
  OnRegisterTypes(c, builder.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await TryMigrateDatabaseAsync(app);

await app.RunAsync();

static async Task TryMigrateDatabaseAsync(IHost host)
{
  using var scope = host.Services.CreateScope();
  var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

  if (hostEnvironment.IsDevelopment() || hostEnvironment.IsStaging())
  {
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
  }
}

void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
{
  services.AddRazorPages();
  services.AddServerSideBlazor();

  services.AddDbContext<IKSqlDBContext, KSqlDBContext>(options =>
  {
    string ksqlDbUrl = Configuration["ksqlDb:Url"];

    var setupParameters = options.UseKSqlDb(ksqlDbUrl);

  }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

  ConfigureEntityFramework(services, Configuration);
}

void ConfigureEntityFramework(IServiceCollection services, IConfiguration Configuration)
{
  var connectionString = Configuration.GetConnectionString("DefaultConnection");

  services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseSqlServer(connectionString); });

  services.AddScoped(p =>
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
      .CreateDbContext());
}

void OnRegisterTypes(ContainerBuilder containerBuilder, IConfiguration Configuration)
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

static void RegisterProducers(ContainerBuilder containerBuilder, string bootstrapServers)
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

static void RegisterConsumers(ContainerBuilder containerBuilder, string bootstrapServers)
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