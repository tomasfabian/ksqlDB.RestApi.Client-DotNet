using System.Text.Json;
using System.Text.Json.Serialization;

namespace ksqlDb.RestApi.Client.Infrastructure.Attributes
{
  [AttributeUsage(AttributeTargets.Enum)]
  public class JsonSnakeCaseStringEnumConverterAttribute<TEnum> : JsonConverterAttribute where TEnum : struct, Enum
  {
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
      return new JsonStringEnumConverter<TEnum>(JsonNamingPolicy.SnakeCaseLower);
    }
  }
}
