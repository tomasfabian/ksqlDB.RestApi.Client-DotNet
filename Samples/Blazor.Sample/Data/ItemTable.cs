using System.Runtime.Serialization;

namespace Blazor.Sample.Data
{
  [DataContract]
  public record ItemTable
  {
    [DataMember(Name = "COUNT")]
    public int Count { get; set; }
  }
}