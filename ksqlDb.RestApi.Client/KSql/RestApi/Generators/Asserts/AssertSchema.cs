namespace ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;

internal static class AssertSchema
{
  public static string CreateStatement(bool exists, AssertSchemaOptions options)
  {
    var notExists = exists ? string.Empty : "NOT EXISTS ";

    var subject = string.IsNullOrEmpty(options.SubjectName) ? string.Empty : $" SUBJECT '{options.SubjectName}'";

    var idValue = options.Id.HasValue ? $" ID {options.Id.Value}" : string.Empty;

    var timeOut = options.Timeout != null ? $" TIMEOUT {options.Timeout.Value} {options.Timeout.TimeUnit}" : string.Empty;

    var statement = $"ASSERT {notExists}SCHEMA{subject}{idValue}{timeOut};";

    return statement;
  }
}
