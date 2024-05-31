using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  internal interface IMetadataProvider
  {
    internal IEnumerable<EntityMetadata> GetEntities();
    IDictionary<Type, IConventionConfiguration> Conventions { get; }
  }
}
