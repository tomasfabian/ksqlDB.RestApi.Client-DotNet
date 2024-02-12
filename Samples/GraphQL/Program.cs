using GraphQL.Movies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGraphQLServer()
  .AddInMemorySubscriptions()
  .AddQueryType()
  .AddTypeExtension<Queries>()
  .AddSubscriptionType()
  .AddTypeExtension<Subscription>();

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
