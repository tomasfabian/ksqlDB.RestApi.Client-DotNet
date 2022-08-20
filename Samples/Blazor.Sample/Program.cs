using Autofac;
using Autofac.Extensions.DependencyInjection;
using Blazor.Sample.Data;
using Blazor.Sample.Extensions.Autofac;
using Blazor.Sample.Extensions.DependencyInjection;
using Blazor.Sample.HostedServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Configuration.AddEnvironmentVariables();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHostedService<IoTSimulatorService>();

builder.Services.ConfigureServices(builder.Configuration);

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
  containerBuilder.OnRegisterTypes(builder.Configuration);
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