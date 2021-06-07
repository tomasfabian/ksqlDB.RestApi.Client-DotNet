using System.Runtime.Serialization;

namespace Blazor.Sample.Data
{
  [DataContract]
  public record ItemStream
  {
    public int Id { get; set; }

    [DataMember(Name = "DESCRIPTION")]
    public string Description { get; set; }
  }
}