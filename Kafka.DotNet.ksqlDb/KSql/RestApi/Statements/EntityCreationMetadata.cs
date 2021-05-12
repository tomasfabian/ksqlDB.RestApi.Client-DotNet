using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  public record EntityCreationMetadata : CreationMetadata, IEntityCreationProperties
  {
    public EntityCreationMetadata()
    {
      ValueFormat = SerializationFormats.Json;
    }

    public string EntityName { get; set; }

    public bool ShouldPluralizeEntityName { get; set; } = true;

    /// <summary>
    /// By default, the topic is assumed to contain non-windowed data. If the data is windowed, i.e., was created using ksqlDB using a query that contains a WINDOW clause, then the WINDOW_TYPE property can be used to provide the window type. Valid values are SESSION, HOPPING, and TUMBLING.
    /// </summary>
    public WindowType? WindowType { get; set; }
    
    /// <summary>
    /// By default, the topic is assumed to contain non-windowed data. If the data is windowed, i.e., was created using ksqlDB using a query that contains a WINDOW clause, and the WINDOW_TYPE property is TUMBLING or HOPPING, then the WINDOW_SIZE property should be set. The property is a string with two literals, window size (a number) and window size unit (a time unit). For example: 10 SECONDS.
    /// </summary>
    public string WindowSize { get; set; }
  }
}