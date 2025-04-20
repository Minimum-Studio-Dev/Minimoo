using Minimoo.Packet;
using Newtonsoft.Json;

namespace Minimoo.Network.WebSockets
{
    public class WebSocketMessage
    {
        public string SessionKey { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; } = false;

        public WebSocketMessage() { }

        public WebSocketMessage(string sessionKey, string id, string message, bool isEncrypted)
        {
            SessionKey = sessionKey;
            Id = id;
            Message = message;
            IsEncrypted = isEncrypted;
        }
    }
}