using System;
using System.Collections.Generic;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements
{
  internal static class CreateStatements
  {
    internal static string GenerateWithClause(EntityCreationMetadata metadata)
    {
      if (metadata == null) throw new ArgumentNullException(nameof(metadata));
		
      var properties = new List<string>();

      if (metadata.WindowType.HasValue)
        properties.Add(@$"WINDOW_TYPE='{metadata.WindowType}'");

      if (metadata.WindowSize.IsNotNullOrEmpty())
        properties.Add(@$"WINDOW_SIZE='{metadata.WindowSize}'");
			
      return GenerateWithClause(metadata, properties);
    }

    internal static string GenerateWithClause(CreationMetadata metadata, IList<string> properties = null) {
      if (metadata == null) throw new ArgumentNullException(nameof(metadata));
      
      properties ??= new List<string>();

      if (metadata.KafkaTopic.IsNotNullOrEmpty())
        properties.Add(@$"KAFKA_TOPIC='{metadata.KafkaTopic}'");

      if (metadata.KeyFormat.HasValue)
        properties.Add(@$"KEY_FORMAT='{metadata.KeyFormat}'");

      if (metadata.ValueFormat.HasValue)
        properties.Add(@$"VALUE_FORMAT='{metadata.ValueFormat}'");

      if (metadata.ValueDelimiter.IsNotNullOrEmpty())
        properties.Add(@$"VALUE_DELIMITER='{metadata.ValueDelimiter}'");

      if (metadata.Partitions.HasValue)
        properties.Add(@$"PARTITIONS='{metadata.Partitions}'");

      if (metadata.Replicas.HasValue)
        properties.Add(@$"REPLICAS='{metadata.Replicas}'");

      if (metadata.Timestamp.IsNotNullOrEmpty())
        properties.Add(@$"TIMESTAMP='{metadata.Timestamp}'");

      if (metadata.TimestampFormat.IsNotNullOrEmpty())
        properties.Add(@$"TIMESTAMP_FORMAT='{metadata.TimestampFormat}'");

      if (metadata.WrapSingleValue.HasValue)
        properties.Add(@$"WRAP_SINGLE_VALUE='{metadata.WrapSingleValue}'");
		
      string result = string.Join(", ", properties);
		
      if(!string.IsNullOrEmpty(result))
        result = $" WITH ( {result} )";
			
      return result;
    }
  }
}