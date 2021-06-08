namespace Blazor.Sample.Kafka
{
  public static class TopicNames
  {
    public static string Items => "Items";
    public static string ItemsStream => "ItemsStream".ToUpper();
    public static string ItemsTable => "ItemsTable".ToUpper();
  }
}