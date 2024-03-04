using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal static class CreateStatements
{
  internal static string GenerateWithClause(EntityCreationMetadata metadata)
  {
#if NETSTANDARD
    if (metadata == null) throw new ArgumentNullException(nameof(metadata));
#else
    ArgumentNullException.ThrowIfNull(metadata);
#endif
		
    var properties = new List<string>();

    if (metadata.WindowType.HasValue)
      properties.Add($"WINDOW_TYPE='{metadata.WindowType}'");

    if (metadata.WindowSize.IsNotNullOrEmpty())
      properties.Add($"WINDOW_SIZE='{metadata.WindowSize}'");
			
    return GenerateWithClause(metadata, properties);
  }

  internal static string GenerateWithClause(CreationMetadata metadata, IList<string> properties = null) {
#if NETSTANDARD
    if (metadata == null) throw new ArgumentNullException(nameof(metadata));
#else
    ArgumentNullException.ThrowIfNull(metadata);
#endif

    properties ??= new List<string>();

    if (metadata.KafkaTopic.IsNotNullOrEmpty())
      properties.Add($"KAFKA_TOPIC='{metadata.KafkaTopic}'");

    if (metadata.KeyFormat.HasValue)
      properties.Add($"KEY_FORMAT='{metadata.KeyFormat}'");

    if (metadata.ValueFormat.HasValue)
      properties.Add($"VALUE_FORMAT='{metadata.ValueFormat}'");

    if (metadata.ValueDelimiter.IsNotNullOrEmpty())
      properties.Add($"VALUE_DELIMITER='{metadata.ValueDelimiter}'");

    if (metadata.Partitions.HasValue)
      properties.Add($"PARTITIONS='{metadata.Partitions}'");

    if (metadata.Replicas.HasValue)
      properties.Add($"REPLICAS='{metadata.Replicas}'");

    if (metadata.Timestamp.IsNotNullOrEmpty())
      properties.Add($"TIMESTAMP='{metadata.Timestamp}'");

    if (metadata.TimestampFormat.IsNotNullOrEmpty())
      properties.Add($"TIMESTAMP_FORMAT='{metadata.TimestampFormat}'");

    if (metadata.WrapSingleValue.HasValue)
      properties.Add($"WRAP_SINGLE_VALUE='{metadata.WrapSingleValue}'");

    if (metadata.KeySchemaId.HasValue)
      properties.Add($"KEY_SCHEMA_ID={metadata.KeySchemaId}");

    if (metadata.ValueSchemaId.HasValue)
      properties.Add($"VALUE_SCHEMA_ID={metadata.ValueSchemaId}");

    if (metadata.KeySchemaFullName.IsNotNullOrEmpty())
      properties.Add($"KEY_SCHEMA_FULL_NAME={metadata.KeySchemaFullName}");

    if (metadata.ValueSchemaFullName.IsNotNullOrEmpty())
      properties.Add($"VALUE_SCHEMA_FULL_NAME={metadata.ValueSchemaFullName}");

    if (metadata.RetentionMs.HasValue)
      properties.Add($"RETENTION_MS={metadata.RetentionMs}");

    string result = string.Join(", ", properties);
		
    if(!string.IsNullOrEmpty(result))
      result = $" WITH ( {result} )";
			
    return result;
  }
}
