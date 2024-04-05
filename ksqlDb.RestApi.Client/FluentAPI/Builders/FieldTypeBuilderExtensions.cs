namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  public static class FieldTypeBuilderExtensions
  {
    public static IFieldTypeBuilder<decimal> Decimal(this IFieldTypeBuilder<decimal> builder, short precision, short scale)
    {
      ((DecimalFieldTypeBuilder<decimal>)builder).Configure(precision, scale);
      return builder;
    }
  }
}
