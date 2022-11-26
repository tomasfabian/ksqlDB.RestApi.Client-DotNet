namespace SqlServer.Connector.Cdc
{
  [Flags]
  public enum ChangeDataCaptureType
  {
    Read,
    Created,
    Updated,
    Deleted
  }
}
