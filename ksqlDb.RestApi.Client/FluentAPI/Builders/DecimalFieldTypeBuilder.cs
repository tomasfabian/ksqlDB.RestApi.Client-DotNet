using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  internal sealed class DecimalFieldTypeBuilder<TProperty>(DecimalFieldMetadata fieldMetadata)
    : FieldTypeBuilder<TProperty>(fieldMetadata)
  {
    internal FieldTypeBuilder<TProperty> Configure(short precision, short scale)
    {
      fieldMetadata.Precision = precision;
      fieldMetadata.Scale = scale;
      return this;
    }
  }
}
