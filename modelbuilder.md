# Model builder

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
    private record Payment
    {
      public string Id { get; set; } = null!;
      public decimal Amount { get; set; }
      public string Description { get; set; } = null!;
    }
    private record Account
    {
      public string Id { get; set; } = null!;
      public decimal Balance { get; set; }
    }

    private static void InitModel()
    {
      ModelBuilder builder = new();

      builder.Entity<Account>()
        .HasKey(c => c.Id);

      builder.Entity<Payment>()
        .HasKey(c => c.Id)
        .Property(b => b.Amount)
        .Decimal(precision: 10, scale: 2);

      var httpClient = new HttpClient
      {
        BaseAddress = new Uri("http://localhost:8088")
      };
      var httpClientFactory = new HttpClientFactory(httpClient);

      var restApiProvider = new KSqlDbRestApiClient(httpClientFactory);
    }
  }
}
```

`builder`: An instance of `ModelBuilder` used to configure the model.
`Entity<T>`: A method that configures an entity of type T.
`HasKey(expression)`: A method used to specify the **primary key** for the entity. It takes a lambda expression that specifies the property or properties that make up the primary key.
`Property(expression)`: A method used to specify a property of the entity. It takes a lambda expression that specifies the property.
`Decimal(precision, scale)`: A method used to specify **precision** and **scale** for a **decimal** property. Precision specifies the maximum number of digits, and scale specifies the number of digits to the right of the decimal point.

## IFromItemTypeConfiguration

To apply configurations using the provided `ModelBuilder`, follow these steps:

- Define Configuration Class: Create a class that implements the appropriate configuration interface. In this case, `PaymentConfiguration` implements `IFromItemTypeConfiguration<Payment>`. 
This class contains configuration logic for the `Payment` entity.

- Configure Properties: Within the `Configure` method of your configuration class, use the provided `IEntityTypeBuilder` to configure the properties of the entity. For instance, the code snippet provided configures the `Amount` property of the `Payment` entity to have a precision of 14 digits and a scale of 14.

- Apply Configuration: Instantiate a `ModelBuilder` object. Then, use the `Apply` method to apply the configuration defined in the `PaymentConfiguration` class to the `ModelBuilder`.

Here's the code snippet demonstrating the application of configurations:

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
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
ModelBuilder builder = new();
builder.Apply(new PaymentConfiguration());
```
