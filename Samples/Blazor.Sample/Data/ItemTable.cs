using System.Runtime.Serialization;

namespace Blazor.Sample.Data
{
  [DataContract]
  public record ItemTable
  {
    public int Id { get; set; }

    [DataMember(Name = "COUNT")]
    public int Count { get; set; }
  }
}