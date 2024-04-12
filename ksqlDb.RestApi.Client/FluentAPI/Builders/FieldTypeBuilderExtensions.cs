namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  /// <summary>
  /// Provides extension methods for configuring fields.
  /// </summary>
  public static class FieldTypeBuilderExtensions
  {
    /// <summary>
    /// Configures a decimal field with the specified precision and scale.
    /// </summary>
    /// <param name="builder">The field type builder for decimal.</param>
    /// <param name="precision">The total number of digits in the decimal value.</param>
    /// <param name="scale">The number of digits to the right of the decimal point.</param>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    public static IFieldTypeBuilder<decimal> Decimal(this IFieldTypeBuilder<decimal> builder, short precision, short scale)
    {
      ((DecimalFieldTypeBuilder<decimal>)builder).Configure(precision, scale);
      return builder;
    }
  }
}
