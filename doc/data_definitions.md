# Data definitions

### Access record header data (v1.6.0)
Starting in ksqlDB 0.24, you can mark a column with [HEADERS](https://docs.ksqldb.io/en/latest/reference/sql/data-definition/#headers) or `HEADER('<key>')` to indicate that it is populated by the header field of the underlying Kafka record.


```C#
[Struct]
internal record KeyValue
{
  public string Key { get; set; }
  public byte[] Value { get; set; }
}

internal class ValueWithHeader
{
  [Headers]
  public KeyValue[] H1 { get; set; }
}
```

The KeyValue record type has to be annotated with the StructAttribute:
```
H1 ARRAY<STRUCT<Key VARCHAR, Value BYTES>> HEADERS
```

```C#
internal class BytesHeader
{
  [Headers("abc")]
  public byte[] H1 { get; set; }
}
```

KSQL:
```
H1 BYTES HEADER('abc')
```

```C#
var sensor = new IoTSensor
{
  SensorId = "Sensor-6",
  Value = 11
};

var message = new Message<string, IoTSensor>
{
  Key = "Sensor-6",
  Value = sensor,
  Headers = new Headers
  {
    new Header("abc", Encoding.UTF8.GetBytes("et"))
  }
};

var deliveryResult = await producer.ProduceAsync(TopicName, message, cancellationToken);
```

```C#
CREATE STREAM sensors (
  sensorId VARCHAR,
  headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>> HEADERS
) WITH (
	KAFKA_TOPIC = 'IoTSensors',
	VALUE_FORMAT = 'JSON',
	PARTITIONS = 1
);

CREATE STREAM sensors (
  sensorId VARCHAR,
  headers BYTES HEADER('abc')
) WITH (
	KAFKA_TOPIC = 'IoTSensors',
	VALUE_FORMAT = 'JSON',
	PARTITIONS = 1
);

SELECT * FROM sensors EMIT CHANGES;
```

```
+----------------------------------------------------------+----------------------------------------------------------+
|SENSORID                                                  |HEADERS                                                   |
+----------------------------------------------------------+----------------------------------------------------------+
|Sensor-6                                                  |ZXQ=                                                      |
|Sensor-4                                                  |ZXQ=                                                      |
```
