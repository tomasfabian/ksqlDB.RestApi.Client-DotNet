namespace ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration
{
  /// <summary>
  /// Represents the configuration for a convention.
  /// </summary>
  public interface IConventionConfiguration
  {
    /// <summary>
    /// Gets the type associated with this convention configuration.
    /// </summary>
    public Type Type { get; }
  }
}
