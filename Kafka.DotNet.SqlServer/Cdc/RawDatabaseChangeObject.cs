using Kafka.DotNet.SqlServer.Cdc.Extensions;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public record RawDatabaseChangeObject
  {
    public string Before { get; set; }
    public string After { get; set; }
    public string Source { get; set; }
    
    public string Op { get; set; }

    public ChangeDataCaptureType OperationType => Op.ToChangeDataCaptureType();
  }

  public record RawDatabaseChangeObject<TEntity> : RawDatabaseChangeObject
  {
    public TEntity EntityBefore
    {
      get
      {
        if (string.IsNullOrEmpty(Before))
          return default;

        var entity = DeserializeValue(Before);

        return entity;
      }
    }

    public TEntity EntityAfter
    {
      get
      {
        if (string.IsNullOrEmpty(After))
          return default;

        var entity = DeserializeValue(After);

        return entity;
      }
    }

    protected virtual TEntity DeserializeValue(string value)
    {
      var entity = System.Text.Json.JsonSerializer.Deserialize<TEntity>(value);

      return entity;
    }
  }
}