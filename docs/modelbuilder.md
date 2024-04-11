# Model builder

Using the **fluent API** with **POCO**s for generated classes that cannot be changed due to code regeneration offers a structured and maintainable approach to configuring your domain model, while keeping your codebase clean and flexible.

The `ModelBuilder` class provides functionalities to define mappings for entities.
The configurations made using the fluent API will override any attributes defined on the entity or its properties.
Use the `Entity<T>()` method of `ModelBuilder` to configure entities and define their properties.

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;

namespace ksqlDB.RestApi.Client.Samples.Model
{
  public class PaymentModelBuilder
  {
    public static async Task InitModelAndCreateStreamAsync(CancellationToken cancellationToken = default)
    {
      ModelBuilder builder = new();
    
      builder.Entity<Account>()
        .HasKey(c => c.Id)
        .Property(b => b.Balance);
    
      builder.Entity<Payment>()
        .HasKey(c => c.Id)
        .Property(b => b.Amount)
        .Decimal(precision: 10, scale: 2);
    
      builder.Entity<Payment>()
        .Property(b => b.Secret)
        .Ignore();

      var httpClient = new HttpClient
      {
        BaseAddress = new Uri("http://localhost:8088")
      };
      var httpClientFactory = new HttpClientFactory(httpClient);
      var restApiProvider = new KSqlDbRestApiClient(httpClientFactory, builder);
      
      var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 1);
      
      var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
      var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
    }
  }
}
```

```C#
private record Payment
{
  public string Id { get; set; } = null!;
  public decimal Amount { get; set; }
  public string Description { get; set; } = null!;
  public string Secret { get; set; } = null!;
}

private record Account
{
  public string Id { get; set; } = null!;
  public decimal Balance { get; set; }
}
```

`builder`: An instance of `ModelBuilder` used to configure the model.

`Entity<T>`: A method that configures an entity of type T.

`HasKey(expression)`: A method used to specify the **primary key** for the entity. It takes a lambda expression that specifies the property that make up the primary key.

`Property(expression)`: A method used to specify a property of the entity. It takes a lambda expression that specifies the property.

`Decimal(precision, scale)`: A method used to specify **precision** and **scale** for a **decimal** property. Precision specifies the maximum number of digits, and scale specifies the number of digits to the right of the decimal point.

`Ignore()`: A method used to specify that a particular property of an entity should be ignored during code generation.

## IFromItemTypeConfiguration

To apply configurations using the provided `ModelBuilder`, follow these steps:

- Define Configuration Class: Create a class that implements the appropriate configuration interface. In this case, `PaymentConfiguration` implements `IFromItemTypeConfiguration<Payment>`. 
This class contains configuration logic for the `Payment` entity.

- Configure Properties: Within the `Configure` method of your configuration class, use the provided `IEntityTypeBuilder` to configure the properties of the entity. For instance, the code snippet provided configures the `Amount` property of the `Payment` entity to have a precision of 14 digits and a scale of 14.

- Apply Configuration: Instantiate a `ModelBuilder` object. Then, use the `Apply` method to apply the configuration defined in the `PaymentConfiguration` class to the `ModelBuilder`.

Here's the code snippet demonstrating the application of configurations:

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;

public class PaymentConfiguration : IFromItemTypeConfiguration<Payment>
{
  public void Configure(IEntityTypeBuilder<Payment> builder)
  {
    builder.Property(b => b.Amount)
      .Decimal(precision: 14, scale: 14);
  }
}
```

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;

ModelBuilder builder = new();
builder.Apply(new PaymentConfiguration());
```

## ModelBuilder conventions
You can add a global convention to the model builder for the **decimal** type in the following way.

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;

var modelBuilder = new ModelBuilder();
var decimalTypeConvention = new DecimalTypeConvention(14, 14);
modelBuilder.AddConvention(decimalTypeConvention);
```
