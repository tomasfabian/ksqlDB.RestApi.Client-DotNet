namespace ksqlDb.RestApi.Client.Metadata
{
  public static class FieldTypeBuilderExtensions
  {
    public static IFieldTypeBuilder<decimal> Decimal(this IFieldTypeBuilder<decimal> builder, short precision, short scale)
    {
      ((DecimalFieldTypeBuilder<decimal>)builder).PrecisionInt(precision, scale);
      return builder;
    }
  }
}
