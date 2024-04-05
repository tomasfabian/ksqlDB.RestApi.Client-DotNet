using System.Linq.Expressions;

namespace ksqlDb.RestApi.Client.Metadata
{
  public class ModelBuilder
  {
    private readonly IDictionary<Type, EntityTypeBuilder> builders = new Dictionary<Type, EntityTypeBuilder>();

    internal IEnumerable<EntityMetadata> GetEntities()
    {
      return builders.Values.Select(c => c.Metadata);
    }

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
