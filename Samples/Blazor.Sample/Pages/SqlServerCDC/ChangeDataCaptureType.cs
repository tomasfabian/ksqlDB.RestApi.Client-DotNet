using System;

namespace Blazor.Sample.Pages.SqlServerCDC
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