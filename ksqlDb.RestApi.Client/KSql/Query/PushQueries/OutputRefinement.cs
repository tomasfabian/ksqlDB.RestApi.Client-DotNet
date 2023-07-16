namespace ksqlDb.RestApi.Client.KSql.Query.PushQueries;

/// <summary>
/// Specifies the output refinement for push queries.
/// </summary>
public enum OutputRefinement
{
  /// <summary>
  /// This is the standard output refinement for push queries, for when you would like to see all changes happening.
  /// </summary>
  Emit = 0,

  /// <summary>
  /// Use the EMIT FINAL output refinement when you want to emit only the final result of a windowed aggregation and suppress the intermediate results until the window closes. This output refinement is supported only for windowed aggregations.
  /// Added in ksqldb 0.28.2 
  /// </summary>
  Final = 1
}
