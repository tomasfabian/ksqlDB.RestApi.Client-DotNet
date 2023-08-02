# Data definitions

### Access record header data (v1.6.0)
To indicate that a column is populated by the header field of the underlying Kafka record, you have the option to either mark it with the keyword `"HEADERS"` or use the syntax `"HEADER('<key>')"`.
If you choose to mark a column with `"HEADERS"`, it should be of the type `ARRAY<STRUCT<key STRING, value BYTES>>` and it will contain the complete list of header keys and values from the Kafka record.

In `ksqlDB.RestApi.Client` you can annotate the corresponding properties with the `HeadersAttribute`:

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

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

The `KeyValue` record type has to be annotated with the `StructAttribute`:
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

The following code snippet creates an instance of an `IoTSensor` object named sensor:
 
```C#
var sensor = new IoTSensor
{
  SensorId = "Sensor-6",
  Value = 11
};
```

Then, a `Message<string, IoTSensor>` object named message is created with the Key set to "Sensor-6", the Value set to the sensor object, and a single header named "abc" with the value "et" encoded in UTF-8.

Finally, the message is asynchronously produced to the TopicName using the `producer.ProduceAsync` method:
```C#
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

The `ksqlDB` stream could have been created and queried using the following KSQL statements:
```SQL
CREATE STREAM sensors (
  sensorId VARCHAR,
  headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>> HEADERS
) WITH (
	KAFKA_TOPIC = 'IoTSensors',
	VALUE_FORMAT = 'JSON',
	PARTITIONS = 1
);

SELECT * FROM sensors EMIT CHANGES;
```

Example output:
```
+----------------------------------------------------------+----------------------------------------------------------+
|SENSORID                                                  |HEADERS                                                   |
+----------------------------------------------------------+----------------------------------------------------------+
|Sensor-6                                                  |ZXQ=                                                      |
|Sensor-4                                                  |ZXQ=                                                      |
```

Alternative approach with `HEADER`: 
```SQL
CREATE STREAM sensors (
  sensorId VARCHAR,
  headers BYTES HEADER('abc')
) WITH (
	KAFKA_TOPIC = 'IoTSensors',
	VALUE_FORMAT = 'JSON',
	PARTITIONS = 1
);
```
