using ProtoBuf;

namespace ksqlDB.Api.Client.Samples.ProtoBuf;

[ProtoContract]
internal record MovieProto
{
  [ProtoMember(1)]
  public string Title { get; set; } = null!;

  [ProtoMember(2)]
  public int Id { get; set; }

  [ProtoMember(3)]
  public int Release_Year { get; set; }
}