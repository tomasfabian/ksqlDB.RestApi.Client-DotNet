namespace Blazor.Sample.Kafka
{
  public static class TopicNames
  {
    public static string IotSensors => "IoTSensors";

    public static string SensorsStream => "SensorsStream".ToUpper();
    public static string SensorsTable => "SensorsTable".ToUpper();
  }
}