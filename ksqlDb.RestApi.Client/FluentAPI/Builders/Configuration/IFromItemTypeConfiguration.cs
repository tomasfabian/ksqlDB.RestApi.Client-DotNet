namespace ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration
{
  /// <summary>
  /// Represents a configuration interface for configuring properties of an entity type.
  /// </summary>
  /// <typeparam name="TEntity">The type of entity to configure.</typeparam>
  public interface IFromItemTypeConfiguration<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Configures the properties of the specified entity type.
    /// </summary>
    /// <param name="builder">The entity type builder used for configuration.</param>
    public void Configure(IEntityTypeBuilder<TEntity> builder);
  }
}
