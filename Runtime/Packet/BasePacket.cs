using Newtonsoft.Json;
using Minimoo.Common.Enum;

namespace Minimoo.Packet
{
    [System.Serializable]
    public abstract class BasePacket
    {
        public virtual int Version { get; } = 1;

        [JsonIgnore]
        public abstract ServerType ServerType { get; }

        [JsonIgnore]
        public abstract string URI { get; }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public abstract class BasePacketResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class BaseWebSocketPacket
    {
        public string Id { get; set; }

        public BaseWebSocketPacket() { }

        public BaseWebSocketPacket(string id) : this()
        {
            Id = id;
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class BaseWebSocketPacketResult : BaseWebSocketPacket
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackMessage { get; set; }

        public BaseWebSocketPacketResult() : base() { }

        public BaseWebSocketPacketResult(string id) : base(id) { }
    }

    public class BaseWebSocketPacketNotify : BaseWebSocketPacket
    {
        public BaseWebSocketPacketNotify() : base() { }

        public BaseWebSocketPacketNotify(string id) : base(id) { }
    }
}
