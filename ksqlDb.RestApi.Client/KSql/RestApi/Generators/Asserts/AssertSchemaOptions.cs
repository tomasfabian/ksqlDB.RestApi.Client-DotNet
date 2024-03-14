using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;

public record AssertSchemaOptions
{
  public AssertSchemaOptions(string subjectName, int? id = null)
  {
    if (string.IsNullOrEmpty(subjectName) && !id.HasValue)
      throw new ArgumentException("Assert schema statements must include a subject name or id");

    SubjectName = subjectName;
    Id = id;
  }

  public string SubjectName { get; }
  
  public int? Id { get; }

  public Duration? Timeout { get; set; }
}
