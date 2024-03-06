namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

#pragma warning disable IDE0060, CS0660, CS0661
public readonly struct Bounds
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
#pragma warning restore IDE0060, CS0660, CS0661
