using System;

namespace Minimoo.Network.WebSockets
{

    [AttributeUsage(AttributeTargets.Method)]
    public class WebSocketMessageMapping : Attribute
    {
        public Type PacketType { get; set; } = null;
    }
}
