# Data types

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
