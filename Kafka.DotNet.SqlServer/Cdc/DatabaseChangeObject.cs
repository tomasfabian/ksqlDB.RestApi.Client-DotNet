namespace Kafka.DotNet.SqlServer.Cdc
{
  public record DatabaseChangeObject<TEntity>
  {
    public TEntity Before { get; set; }
    public TEntity After { get; set; }
    public Source Source { get; set; }
    public string Op { get; set; }
    public long TsMs { get; set; }
    public object Transaction { get; set; }
  }
}