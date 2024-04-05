namespace ksqlDb.RestApi.Client.Metadata
{
  internal class DecimalFieldTypeBuilder<TProperty>(DecimalFieldMetadata fieldMetadata)
    : FieldTypeBuilder<TProperty>(fieldMetadata)
  {
    internal FieldTypeBuilder<TProperty> PrecisionInt(short precision, short scale)
    {
      fieldMetadata.Precision = precision;
      fieldMetadata.Scale = scale;
      return this;
    }
  }
}
