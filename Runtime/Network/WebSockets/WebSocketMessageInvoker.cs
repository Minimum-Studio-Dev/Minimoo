using System;
using System.Reflection;

namespace Minimoo.Network.WebSockets
{
    public class WebSocketMessageInvoker
    {
        public IWebSocketMessageReceiver MessagerReciever { get; set; }

        public Type PacketType { get; set; }

        public MethodInfo Method { get; set; }

        public WebSocketMessageInvoker(IWebSocketMessageReceiver reciever, Type packetType, MethodInfo method)
        {
            MessagerReciever = reciever;
            PacketType = packetType;
            Method = method;
        }
    }
}
