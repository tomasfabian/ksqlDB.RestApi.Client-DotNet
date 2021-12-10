using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters
{
  public class TimeSpanToStringConverter : JsonConverter<TimeSpan>
  {
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var value = reader.GetString();

      return TimeSpan.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
      string convertedValue = value.ToString();

      writer.WriteStringValue(convertedValue);
    }
  }
}