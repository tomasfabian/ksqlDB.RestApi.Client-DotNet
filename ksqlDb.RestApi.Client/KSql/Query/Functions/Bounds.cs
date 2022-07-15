namespace ksqlDB.RestApi.Client.KSql.Query.Functions;
#pragma warning disable CS0660, CS0661
public struct Bounds
#pragma warning restore CS0660, CS0661
{
  public static readonly Bounds WindowStart = new();
  public static readonly Bounds WindowEnd = new();

  public static bool operator <(Bounds bounds, string value)
  {
    return true;
  }

  public static bool operator >(Bounds bounds, string value)
  {
    return true;
  }

  public static bool operator <(Bounds bounds, long value)
  {
    return true;
  }

  public static bool operator >(Bounds bounds, long value)
  {
    return true;
  }


  public static bool operator <=(Bounds bounds, string value)
  {
    return true;
  }

  public static bool operator >=(Bounds bounds, string value)
  {
    return true;
  }

  public static bool operator <=(Bounds bounds, long value)
  {
    return true;
  }

  public static bool operator >=(Bounds bounds, long value)
  {
    return true;
  }

  public static bool operator ==(Bounds bounds, long value)
  {
    return true;
  }

  public static bool operator ==(Bounds bounds, string value)
  {
    return true;
  }

  public static bool operator !=(Bounds bounds, long value)
  {
    return true;
  }

  public static bool operator !=(Bounds bounds, string value)
  {
    return true;
  }
}