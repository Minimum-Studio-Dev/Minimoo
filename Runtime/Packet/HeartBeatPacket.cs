
using Minimoo.Common.Enum;

namespace Minimoo.Packet
{
    public sealed class HeartBeatHttpReq : BasePacket
    {
        public override ServerType ServerType { get; } = ServerType.Lobby;
        public override string URI { get; } = "HeartBeat/HeartBeat";
    }

    public sealed class HeartBeatHttpRes : BasePacketResult
    {
    }

    public sealed class HeartBeatWebSocketReq : BaseWebSocketPacket
    {
        public HeartBeatWebSocketReq() : base("HeartBeatReq") { }
    }

    public sealed class HeartBeatWebSocketRes : BaseWebSocketPacketResult
    {
        public HeartBeatWebSocketRes() : base("HeartBeatRes") { }
    }
}
