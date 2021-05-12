namespace Kafka.DotNet.ksqlDB.KSql.Query.Functions
{
  public struct Bounds
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
}