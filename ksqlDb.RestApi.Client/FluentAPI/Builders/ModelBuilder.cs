using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  /// <summary>
  /// Represents a builder for configuring the model.
  /// </summary>
  public class ModelBuilder
  {
    private readonly IDictionary<Type, EntityTypeBuilder> builders = new Dictionary<Type, EntityTypeBuilder>();
    internal readonly IDictionary<Type, IConventionConfiguration> Conventions = new Dictionary<Type, IConventionConfiguration>();

    internal IEnumerable<EntityMetadata> GetEntities()
    {
      return builders.Values.Select(c => c.Metadata);
    }

    /// <summary>
    /// Applies configuration for a specific entity type to the model.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to configure.</typeparam>
    /// <param name="configuration">The configuration to apply.</param>
    /// <returns>The current <see cref="ModelBuilder"/> instance.</returns>
    public ModelBuilder Apply<TEntity>(IFromItemTypeConfiguration<TEntity> configuration)
      where TEntity : class
    {
      configuration.Configure(Entity<TEntity>());

      return this;
    }

    /// <summary>
    /// Adds a convention to the model builder.
    /// </summary>
    /// <param name="configuration">The configuration for the convention to add.</param>
    /// <returns>The current <see cref="ModelBuilder"/> instance.</returns>
    public ModelBuilder AddConvention(IConventionConfiguration configuration)
    {
      Conventions.Add(configuration.Type, configuration);

      return this;
    }

    /// <summary>
    /// Configures an entity type in the model.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to configure.</typeparam>
    /// <returns>The <see cref="ModelBuilder"/> for further configuration.</returns>
    public IEntityTypeBuilder<TEntity> Entity<TEntity>()
      where TEntity : class
    {
      if (builders.ContainsKey(typeof(TEntity)))
        return (EntityTypeBuilder<TEntity>)builders[typeof(TEntity)];

      var builder = (EntityTypeBuilder)Activator.CreateInstance(typeof(EntityTypeBuilder<>).MakeGenericType(typeof(TEntity)))!;

      builders[typeof(TEntity)] = builder ?? throw new Exception($"Failed to create entity type builder for {nameof(TEntity)}");
      return (EntityTypeBuilder<TEntity>)builder;
    }
  }
}
