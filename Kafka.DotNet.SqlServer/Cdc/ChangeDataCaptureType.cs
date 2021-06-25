using System;

namespace Kafka.DotNet.SqlServer.Cdc
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