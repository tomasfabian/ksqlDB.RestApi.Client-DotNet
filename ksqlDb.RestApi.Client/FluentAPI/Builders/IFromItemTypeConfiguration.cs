namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  public interface IFromItemTypeConfiguration<TEntity>
    where TEntity : class
  {
    public void Configure(IEntityTypeBuilder<TEntity> builder);
  }
}
