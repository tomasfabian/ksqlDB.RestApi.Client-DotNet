namespace DataTypes.Model;

public class Dates
{
  public DateTime Dt { get; set; }
  public TimeSpan Ts { get; set; }
  public DateTimeOffset DtOffset { get; set; }
  public long UnixDt => (long)Dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
}
