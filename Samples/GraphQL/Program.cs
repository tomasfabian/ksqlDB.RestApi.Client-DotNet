using GraphQL.ksqlDB;
using GraphQL.Movies;
using GraphQL.Services;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGraphQLServer()
  .AddInMemorySubscriptions()
  .AddQueryType()
  .AddTypeExtension<Queries>()
  .AddSubscriptionType()
  .AddTypeExtension<Subscription>();

var ksqlDbUrl = "http://localhost:8088";

builder.Services.AddDbContext<IMoviesKSqlDbContext, MoviesKSqlDbContext>(
  options =>
  {
    var setupParameters = options.UseKSqlDb(ksqlDbUrl);

    setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

  }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

builder.Services.AddHostedService<MoviesConsumerBackgroundService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();
app.UseWebSockets();

app.Run();
