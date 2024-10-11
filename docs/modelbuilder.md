# Model builder
**v5.0.0**

Using the **fluent API** with **POCO**s for generated classes that cannot be changed due to code regeneration offers a structured and maintainable approach to configuring your domain model, while keeping your codebase clean and flexible.

The `ModelBuilder` class provides functionalities to define mappings for entities.
The configurations made using the fluent API will override any attributes defined on the entity or its properties.
Use the `Entity<T>()` method of `ModelBuilder` to configure entities and define their properties.

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;

public static async Task InitModelAndCreateTableAsync(CancellationToken cancellationToken = default)
{
  ModelBuilder builder = new();

  builder.Entity<Account>()
    .HasKey(c => c.Id)
    .Property(b => b.Balance);

  builder.Entity<Account>()
    .Property(b => b.Secret)
    .Ignore();

  builder.Entity<Payment>()
    .HasKey(c => c.Id)
    .Property(b => b.Amount)
    .Decimal(precision: 10, scale: 2);

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri("http://localhost:8088")
  };
  var httpClientFactory = new HttpClientFactory(httpClient);
  var restApiProvider = new KSqlDbRestApiClient(httpClientFactory, builder);
  
  var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Payment), partitions: 3);
  
  var responseMessage = await restApiProvider.CreateTableAsync<Payment>(entityCreationMetadata, true, cancellationToken);
  var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

  entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Account), partitions: 1)
  {
    Replicas = 3
  };
  responseMessage = await restApiProvider.CreateTableAsync<Account>(entityCreationMetadata, true, cancellationToken);
}
```

```C#
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
  public string Secret { get; set; } = null!;
}
```

`builder`: An instance of `ModelBuilder` used to configure the model.

`Entity<T>`: A method that configures an entity of type T.

`HasKey(expression)`: A method used to specify the **primary key** for the entity. It takes a lambda expression that specifies the property that make up the primary key.

`Property(expression)`: A method used to specify a property of the entity. It takes a lambda expression that specifies the property.

`Decimal(precision, scale)`: A method used to specify **precision** and **scale** for a **decimal** property. Precision specifies the maximum number of digits, and scale specifies the number of digits to the right of the decimal point.

`Ignore()`: A method used to specify that a particular property of an entity should be ignored during code generation.

The `Id` column was designated as the **primary key** for the `Payments` table, reflecting its configuration through the model builder's `HasKey` method for the entity:

```SQL
CREATE TABLE IF NOT EXISTS Payments (
	Id VARCHAR PRIMARY KEY,
	Amount DECIMAL(10,2),
	Description VARCHAR
) WITH ( KAFKA_TOPIC='Payment', VALUE_FORMAT='Json', PARTITIONS='3' );
```

The `Secret` column was excluded from the generated DDL statement due to its configuration using the model builder's `Ignore` method:

```SQL
CREATE TABLE IF NOT EXISTS Accounts (
	Id VARCHAR PRIMARY KEY,
	Balance DECIMAL
) WITH ( KAFKA_TOPIC='Account', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='3' );
```

## Dependency injection
This setup ensures that the `ksqlDB` client, context, and model configuration are properly injected throughout your application.

```C#
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

var builder = WebApplication.CreateBuilder(args);

ModelBuilder modelBuilder = new();

builder.Services.ConfigureKSqlDb(
  builder.Configuration.GetConnectionString("KafkaConnection")!,
  parameters =>
  {
    parameters
      .SetAutoOffsetReset(AutoOffsetReset.Latest)
      .SetIdentifierEscaping(IdentifierEscaping.Always)
      .SetJsonSerializerOptions(options =>
      {
        options.PropertyNameCaseInsensitive = true;
      });
  }
).AddSingleton(modelBuilder);
```

## IFromItemTypeConfiguration
**v5.0.0**

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
**v5.0.0**

You can add a global convention to the model builder for the **decimal** type in the following way.

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;

var modelBuilder = new ModelBuilder();
var decimalTypeConvention = new DecimalTypeConvention(14, 14);
modelBuilder.AddConvention(decimalTypeConvention);
```

### WithHeader
**v5.1.0**

Properties of an entity can be marked as a [HEADER](https://docs.ksqldb.io/en/latest/reference/sql/data-definition/#headers) with the model builder's fluent API as demonstrated below:

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;

string header = "abc";
modelBuilder.Entity<PocoWithHeader>()
  .Property(c => c.Header)
  .WithHeader(header);

var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(PocoWithHeader), partitions: 1)
{
  Replicas = 3
};
var responseMessage = await restApiProvider.CreateStreamAsync<PocoWithHeader>(entityCreationMetadata, true);
    
private record PocoWithHeader
{
  public byte[] Header { get; init; } = null!;
}
```

Here's the equivalent KSQL statement for the described scenario:
```SQL
CREATE STREAM IF NOT EXISTS PocoWithHeaders (
	Header BYTES HEADER('abc')
) WITH ( KAFKA_TOPIC='PocoWithHeader', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='3' );
```

The `WithHeader` function within the fluent API takes precedence over the `HeadersAttribute`.

### WithHeaders
**v5.1.0**

Properties of an entity can be marked as a [HEADERS](https://docs.ksqldb.io/en/latest/reference/sql/data-definition/#headers) with the model builder's fluent API as demonstrated below:

```C#
using ksqlDb.RestApi.Client.Metadata;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

modelBuilder.Entity<Movie>()
  .Property(c => c.Headers)
  .WithHeaders();

var statementContext = new StatementContext
{
  CreationType = CreationType.Create,
  KSqlEntityType = KSqlEntityType.Stream
};

var statement = new CreateEntity(modelBuilder).Print<PocoWithHeaders>(statementContext, creationMetadata, null);

[Struct]
private record KeyValuePair
{
  public string Key { get; set; }
  public byte[] Value { get; set; }
}

private record Movie
{
  public KeyValuePair[] Headers { get; init; } = null!;
}
```

The `StructAttribute` in `ksqlDb.RestApi.Client` marks a class as a [STRUCT type](https://docs.ksqldb.io/en/latest/reference/sql/data-types/#struct) in ksqlDB 	a strongly typed structured data type `org.apache.kafka.connect.data.Struct`.
Without the annotation, the type's name, `KeyValuePair`, would be used in code generation.

The value in the above `statement` variable is equivalent to:
```SQL
CREATE STREAM Movie (
	Headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>> HEADERS
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
```

The `WithHeaders` function within the fluent API takes precedence over the `HeadersAttribute`.

### HasColumnName
**v6.1.0**

The `HasColumnName` function is employed during JSON deserialization and code generation, particularly in tasks like crafting CREATE STREAM or INSERT INTO statements.

The below code demonstrates how to use the `HasColumnName` method in the fluent API to override the property name `Description` to `Desc` during code generation:

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

modelBuilder.Entity<Payment>()
  .Property(b => b.Description)
  .HasColumnName("Desc");

var statement = new CreateInsert(modelBuilder)
    .Generate(payment, new InsertProperties { IdentifierEscaping = IdentifierEscaping.Keywords });
```

The KSQL snippet illustrates an example INSERT statement with the overridden column name, showing how it corresponds to the fluent API configuration:

```SQL
INSERT INTO Payments (Id, Amount, Desc)
VALUES ('1', 33, 'Purchase');
```

### AsSource
**v6.3.0**

The `AsSource` function designates fields in entity types as ksqlDB struct types.

The following code showcases how to use the `AsSource` method in the fluent API to infer the underlying `ksqlDB` type as a struct during code generation:

```C#
private record KeyValuePair
{
  public string Key { get; set; } = null!;
  public byte[] Value { get; set; } = null!;
}

private record Record
{
  public KeyValuePair[] Headers { get; init; } = null!;
}
```

```C#
ModelBuilder builder = new();
 
builder.Entity<Record>()
        .Property(b => b.Headers)
        .AsStruct();
        
var creationMetadata = new EntityCreationMetadata("my_topic", partitions: 3);

var ksql = new StatementGenerator(builder).CreateTable<Record>(creationMetadata, ifNotExists: true);
```

The KSQL snippet illustrates an example CREATE TABLE statement with the injected `STRUCT` type,
showing how it corresponds to the fluent API configuration:

```SQL
CREATE TABLE IF NOT EXISTS Records (
	  Headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>>
) WITH ( KAFKA_TOPIC='my_topic', VALUE_FORMAT='Json', PARTITIONS='3' );
```
