namespace ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration
{
  public class DecimalTypeConvention(short precision, short scale) : IConventionConfiguration
  {
    public short Precision { get; } = precision;
    public short Scale { get; } = scale;
    public Type Type => typeof(decimal);
  }
}
