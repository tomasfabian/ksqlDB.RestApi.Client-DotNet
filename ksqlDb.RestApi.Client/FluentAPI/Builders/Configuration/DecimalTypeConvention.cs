namespace ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration
{
  /// <summary>
  /// Represents a decimal type convention for configuring precision and scale.
  /// </summary>
  public class DecimalTypeConvention : IConventionConfiguration
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalTypeConvention"/> class with the specified precision and scale.
    /// </summary>
    /// <param name="precision">The precision of the decimal type.</param>
    /// <param name="scale">The scale of the decimal type.</param>
    public DecimalTypeConvention(short precision, short scale)
    {
      Precision = precision;
      Scale = scale;
    }

    /// <summary>
    /// Gets the precision of the decimal type.
    /// </summary>
    public short Precision { get; }

    /// <summary>
    /// Gets the scale of the decimal type.
    /// </summary>
    public short Scale { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the decimal type.
    /// </summary>
    public Type Type => typeof(decimal);
  }
}
