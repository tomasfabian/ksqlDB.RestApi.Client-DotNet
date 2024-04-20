using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  internal sealed class BytesArrayFieldTypeBuilder<TProperty>(BytesArrayFieldMetadata fieldMetadata)
    : FieldTypeBuilder<TProperty>(fieldMetadata)
  {
    internal FieldTypeBuilder<TProperty> Configure(string header)
    {
      fieldMetadata.Header = header;
      return this;
    }
  }
}
