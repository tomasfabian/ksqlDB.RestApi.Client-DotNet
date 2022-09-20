using System.Text.Json;
using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;

public sealed class JsonConverterGuid : JsonConverter<Guid>
{
  public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var value = reader.GetString();

    if (value != null)
      return Guid.Parse(value);

    throw new FormatException("The JSON value is not in a supported Guid format.");
  }

  public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value);
  }
}