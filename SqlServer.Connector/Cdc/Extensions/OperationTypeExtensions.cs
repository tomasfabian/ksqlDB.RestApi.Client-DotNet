namespace SqlServer.Connector.Cdc.Extensions
{
  public static class OperationTypeExtensions
  {
    public static ChangeDataCaptureType ToChangeDataCaptureType(this string operation)
    {
      return operation switch
      {
        "r" => ChangeDataCaptureType.Read,
        "c" => ChangeDataCaptureType.Created,
        "u" => ChangeDataCaptureType.Updated,
        "d" => ChangeDataCaptureType.Deleted,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
      };
    }
  }
}
