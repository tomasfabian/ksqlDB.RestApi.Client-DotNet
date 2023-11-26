namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements
{
  // CommandStatus indicates the current state of statement execution.
  public static class CommandStatus
  {
    // The statement was accepted by the server and is being processed.
    public const string Queued = "QUEUED";
    // The statement was accepted by the server and is being processed.
    public const string Parsing = "PARSING";
    // The statement was accepted by the server and is being processed.
    public const string Executing = "EXECUTING";
    // The statement was successfully processed.
    public const string Success = "SUCCESS";
    // There was an error processing the statement.The statement was not executed.
    public const string Error = "ERROR";
    // The query started by the statement was terminated. Only returned for CREATE STREAM|TABLE AS SELECT.
    public const string Terminated = "TERMINATED";
  }
}
