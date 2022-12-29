# Data types

### Supported data types mapping

|     ksql     |            c#            |
|:------------:|:------------------------:|
|    VARCHAR   |          string          |
|    INTEGER   |            int           |
|    BIGINT    |           long           |
|    DOUBLE    |          double          |
|    BOOLEAN   |           bool           |
|     BYTES    |          byte[]          |
|  ```ARRAY``` | C#Type[] or IEnumerable  |
|   ```MAP```  |        IDictionary       |
| ```STRUCT``` |          struct          |
|     DATE     |     System.DateTime      |
|     TIME     |     System.TimeSpan      |
|   TIMESTAMP  |    System.DateTimeOffset |

Array type mapping example:
All of the elements in the array must be of the same type. The element type can be any valid SQL type.
```
ksql: ARRAY<INTEGER>
C#  : int[]
```
Destructuring an array (ksqldb represents the first element of an array as 1):
```C#
queryStream
  .Select(_ => new { FirstItem = new[] {1, 2, 3}[1] })
```
Generates the following KSQL:
```KSQL
ARRAY[1, 2, 3][1] AS FirstItem
```
Array length:
```C#
queryStream
  .Select(_ => new[] {1, 2, 3}.Length)
```
Generates the following KSQL:
```KSQL
ARRAY_LENGTH(ARRAY[1, 2, 3])
```

Struct type mapping example:
A struct represents strongly typed structured data. A struct is an ordered collection of named fields that have a specific type. The field types can be any valid SQL type.
```C#
struct Point
{
  public int X { get; set; }

  public int Y { get; set; }
}

queryStream
  .Select(c => new Point { X = c.X, Y = 2 });
```
Generates the following KSQL:
```KSQL
SELECT STRUCT(X := X, Y := 2) FROM StreamName EMIT CHANGES;
```

Destructure a struct:
```C#
queryStream
  .Select(c => new Point { X = c.X, Y = 2 }.X);
```
```KSQL
SELECT STRUCT(X := X, Y := 2)->X FROM StreamName EMIT CHANGES;
```

### Structs

[Structs](https://docs.ksqldb.io/en/latest/how-to-guides/query-structured-data/#structs)
 are an associative data type that map VARCHAR keys to values of any type. Destructure structs by using arrow syntax (->).
```C#
public struct Point
{
  public int X { get; set; }

  public int Y { get; set; }
}
```

```C#
query
  .Select(c => new Point { X = 1, Y = 2 });
```

```SQL
SELECT STRUCT(X := 1, Y := 2) FROM point EMIT CHANGES;
```

### Maps

[Maps](https://docs.ksqldb.io/en/latest/how-to-guides/query-structured-data/#maps)
are an associative data type that map keys of any type to values of any type. The types across all keys must be the same. The same rule holds for values. Destructure maps using bracket syntax ([]).
```C#
var dictionary = new Dictionary<string, int>()
{
  { "c", 2 },
  { "d", 4 }
};
``` 
```KSQL
MAP('c' := 2, 'd' := 4)
```

Accessing map elements:
```C#
dictionary["c"]
``` 
```KSQL
MAP('c' := 2, 'd' := 4)['d'] 
```
Deeply nested types:
```C#
context.CreateQueryStream<Tweet>()
  .Select(c => new
  {
    Map = new Dictionary<string, int[]>
    {
      { "a", new[] { 1, 2 } },
      { "b", new[] { 3, 4 } },
    }
  });
```
Generated KSQL:
```KSQL
SELECT MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]) Map 
FROM Tweets EMIT CHANGES;
```

### Time types DATE, TIME AND TIMESTAMP
**v1.5.0** (ksqldb 0.20.0)

Documentation for [Time types](https://docs.ksqldb.io/en/0.22.0-ksqldb/reference/sql/data-types/#time-types)

```C#
public record MyClass
{
  public DateTime Dt { get; set; }
  public TimeSpan Ts { get; set; }
  public DateTimeOffset DtOffset { get; set; }
}
```

```C#
var httpResponseMessage = await restApiClient.CreateStreamAsync<MyClass>(metadata);
```

Generated statement:

```SQL
CREATE STREAM MyClasses (
	Dt DATE,
	Ts TIME,
	DtOffset TIMESTAMP
) WITH ( KAFKA_TOPIC='MyClass', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
```

```C#
var value = new MyClass
{
  Dt = new DateTime(2021, 4, 1),
  Ts = new TimeSpan(1, 2, 3),
  DtOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4))
};

httpResponseMessage  = await restApiClient.InsertIntoAsync(value);
```

Generated statement:
```SQL
INSERT INTO MyClasses (Dt, Ts, DtOffset) VALUES ('2021-04-01', '01:02:03', '2021-07-04T13:29:45.447+04:00');
```

```C#
using var subscription = context.CreateQueryStream<MyClass>()
  .Subscribe(onNext: m =>
  {
    Console.WriteLine($"Time types: {m.Dt} : {m.Ts} : {m.DtOffset}");
  }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
```

Output from ksqldb-cli:
```
print 'MyClass' from beginning;
```

_rowtime: 2021/12/11 10:36:55.678 Z, key: `<null>`, value: {"DT":18718,"TS":3723000,"DTOFFSET":1625390985447}, partition: 0_

**NOTE**: 
ksqldb 0.22.0 REST API doesn't contain the offset in the payload for the TIMESTAMP values. Needs further investigation (possible bug in ksqldb).


# System.GUID as ksqldb VARCHAR type (v2.4.0)

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

public record LinkCreated
{
  public string Name { get; set; }

  [Key]
  public Guid AggregateRootId { get; set; }
}
``` 
```C#
EntityCreationMetadata metadata = new()
{
  KafkaTopic = "MyGuids",
  ValueFormat = SerializationFormats.Json,
  Partitions = 1,
  Replicas = 1
};

var httpResponseMessage = await restApiProvider.CreateStreamAsync<LinkCreated>(metadata, ifNotExists: true)
  .ConfigureAwait(false);
```
KSQL:

```SQL
CREATE STREAM IF NOT EXISTS LINKCREATEDS (
  NAME STRING,
  AGGREGATEROOTID STRING KEY
) WITH (KAFKA_TOPIC='MyGuids', KEY_FORMAT='KAFKA', PARTITIONS='1', REPLICAS='1', VALUE_FORMAT='JSON');
```

### BYTES character type and ToBytes string function
**v1.0.0**

- [The bytes type](https://docs.ksqldb.io/en/latest/reference/sql/data-types/#character-types) - represents an array of raw bytes.
- variable-length byte array in C# is represented as byte[]
- requirements: ksqldb 0.21.0

**ToBytes** - Converts a STRING value in the specified encoding to BYTES. The accepted encoders are 'hex', 'utf8', 'ascii' and 'base64'. Since: - ksqldb 0.21

```C#
Expression<Func<Tweet, byte[]>> expression = c => K.Functions.ToBytes(c.Message, "utf8");
```

Is equivalent to:
```KSQL
TO_BYTES(Message, 'utf8')
```

### FromBytes string function
**v1.0.0**

- Converts a BYTES value to STRING in the specified encoding. The accepted encoders are 'hex', 'utf8', 'ascii' and 'base64'.

```C#
struct Thumbnail
{
  public byte[] Image { get; init; }
}
```
```C#
Expression<Func<Thumbnail, string>> expression = c => K.Functions.FromBytes(c.Image, "utf8");
```
Is equivalent to:
```KSQL
FROM_BYTES(Message, 'utf8')
```

### Decimal precision
```C#
class Transaction
{
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Decimal(3, 2)]
  public decimal Amount { get; set; }
}
```
Generated KSQL:
```KSQL
Amount DECIMAL(3,2)
```
