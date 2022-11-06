using ProtoBuf;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.Models;

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