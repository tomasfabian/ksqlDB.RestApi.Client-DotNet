namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Formats;

internal static class ValueFormats
{
  internal static string DateFormat => @"yyyy-MM-dd";
  internal static string TimeFormat => @"hh\:mm\:ss";
  internal static string DateTimeOffsetFormat => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz";
}