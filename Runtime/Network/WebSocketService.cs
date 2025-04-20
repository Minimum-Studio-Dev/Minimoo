using Minimoo.Common.Enum;
using Minimoo.Network.Encrypter;
using Minimoo.Packet;
using Minimoo.Network.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace Minimoo.Network
{
    /// <summary>
    /// websocket 서비스
    /// </summary>
    public class WebSocketService : Singleton<WebSocketService>
    {
        private string url;
        private string sessionKey;
        private bool isEncrypted;

        private Queue<WebSocketMessage> pendingPackets = new Queue<WebSocketMessage>();

        private Queue<WebSocketMessage> sendFailedPackets = new Queue<WebSocketMessage>();

        private WebSocket socket = null;

        private Action<WebSocket, ushort> SucessAction = null;
        private Action<WebSocket, ushort> FailAction = null;

        public List<Component> networkReceivers = new();
        public List<Component> willBeAddReceivers = new();
        public List<Component> willBeRemoveReceivers = new();

        public Dictionary<Component, Dictionary<string, WebSocketMessageInvoker>> messageInvokers = new();

        public long HEART_BEAT_TIME_MS => 1 * 30 * 1000;

        private long _lastHeartbeatTime = 0;
        private PacketEncrypter encrypter = null;

        public void Init(ServerInfo serverInfo, string sessionKey, Action<WebSocket, ushort> sucessAction, Action<WebSocket, ushort> failAction)
        {
            this.url = serverInfo.MakeURI(URIType.WebSocket);
            this.isEncrypted = serverInfo.IsEncrypted;
            this.sessionKey = sessionKey;
            this.SucessAction = sucessAction;
            this.FailAction = failAction;
            this.encrypter = serverInfo.Encrypter;

            socket = new WebSocket(url);

            if (serverInfo.IsSecured)
            {
                // Set SSL protocols explicitly (important for modern policies)
                socket.SslConfiguration.EnabledSslProtocols =
                    System.Security.Authentication.SslProtocols.Tls12;
                // Optionally log debug info for troubleshooting
                socket.Log.Level = WebSocketSharp.LogLevel.Trace;
            }

            socket.OnOpen += OnOpen;

            socket.OnMessage += OnMessage;
            socket.OnError += OnError;
            socket.OnClose += OnClose;
            socket.Connect();
        }

        public void Disconnect()
        {
            Release();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void Release()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        void Heartbeat()
        {
            Send(new HeartBeatWebSocketReq());
        }

        public bool IsOpen()
        {
            return socket != null && socket.ReadyState == WebSocketState.Open;
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            D.Error("WebSocket error : " + e.Message);
            this.FailAction?.Invoke(this.socket, 1);
        }

        private void OnOpen(object sender, EventArgs e)
        {
            D.Log("WebSocket opened.");
            this.SucessAction?.Invoke(this.socket, 0);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            D.Log("WebSocket closed : " + e.Code + ", " + e.Reason);
            this.FailAction?.Invoke(this.socket, e.Code);

            Release();

        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                D.Log($"WebSocket Received : {e.Data}");
                var message = JsonConvert.DeserializeObject<WebSocketMessage>(e.Data);
                pendingPackets.Enqueue(message);
                SendFailedMessages();
            }
            catch (Exception ex)
            {
                D.Error("WebSocket parse error : " + ex.Message + ", " + ex.StackTrace);
            }
        }

        public void SendFailedMessages()
        {
            while (sendFailedPackets.Count != 0)
            {
                var msg = sendFailedPackets.Dequeue();
                Send(msg);
            }
        }

        public void Send(BaseWebSocketPacket packet)
        {
            try
            {
                Send(new WebSocketMessage(sessionKey, packet.Id, packet.ToJson(), isEncrypted));
            }
            catch (Exception ex)
            {
                D.Error("WebSocket parse error : " + ex.Message + ", " + ex.StackTrace);
            }
        }


        public void Send(WebSocketMessage msg)
        {
            if (IsOpen() == false)
            {
                sendFailedPackets.Enqueue(msg);
                return;
            }

            if (msg.IsEncrypted)
                msg.Message = this.encrypter.Encrypt(msg.Message);

            socket.Send(JsonConvert.SerializeObject(msg));
        }

        private void PumpingMessages()
        {
            while (pendingPackets.Count != 0)
            {
                var msg = pendingPackets.Dequeue();
                SendToDispatcher(msg);
            }
        }

        public DateTime TimeNow
        {
            get
            {
                return DateTime.Now;
            }
        }


        protected void Update()
        {
            PumpingMessages();

            var nowMS = NetworkService.Instance.NowUnixMillisecond;

            if (IsOpen())
            {
                if (nowMS - _lastHeartbeatTime > HEART_BEAT_TIME_MS)
                {
                    _lastHeartbeatTime = nowMS;
                    Heartbeat();
                }
            }
            else
            {
                _lastHeartbeatTime = nowMS;
            }
        }

        private void SendToDispatcher(WebSocketMessage msg)
        {
            if (msg.IsEncrypted)
                msg.Message = this.encrypter.Decrypt(msg.Message);
            Dispatch(msg);
        }

        private void PayloadToPendingPacketList(WebSocketMessage msg)
        {
            pendingPackets.Enqueue(msg);
        }

        public void AddReceiver<T>(T receiver) where T : MonoBehaviour, IWebSocketMessageReceiver
        {
            if (!networkReceivers.Contains(receiver))
            {
                willBeAddReceivers.Add(receiver);
            }
        }

        public void RemoveReceiver<T>(T receiver) where T : Component, IWebSocketMessageReceiver
        {
            willBeRemoveReceivers.Remove(receiver);
        }

        private void RegisterInvoker(Component receiver)
        {
            if (messageInvokers.TryAdd(receiver, new Dictionary<string, WebSocketMessageInvoker>()))
            {
                var methods = receiver.GetType().GetMethods();

                foreach (var method in methods)
                {
                    var messageMapping = method.GetCustomAttribute<WebSocketMessageMapping>();

                    if (messageMapping != null)
                    {
                        var wsPacket = Activator.CreateInstance(messageMapping.PacketType) as BaseWebSocketPacket;

                        if (wsPacket != null)
                        {
                            var messageId = wsPacket.Id;

                            messageInvokers[receiver].TryAdd(messageId, new WebSocketMessageInvoker((IWebSocketMessageReceiver)receiver, messageMapping.PacketType, method));
                        }
                    }
                }
            }
        }

        public void Dispatch(WebSocketMessage message)
        {
            D.Log($"Dispatch Message Received : {JsonConvert.SerializeObject(message)}");
            int invoked = 0;

            networkReceivers = networkReceivers.Where(x => x != null).ToList();

            for (int i = 0; i < willBeAddReceivers.Count; ++i)
            {
                var add = willBeAddReceivers[i];

                if (!networkReceivers.Contains(add))
                {
                    networkReceivers.Add(add);
                    RegisterInvoker(add);
                }
            }
            willBeAddReceivers.Clear();

            for (int i = 0; i < willBeRemoveReceivers.Count; ++i)
            {
                var remove = willBeRemoveReceivers[i];
                networkReceivers.Remove(remove);
                messageInvokers.Remove(remove);
            }
            willBeRemoveReceivers.Clear();

            for (int i = 0; i < networkReceivers.Count; i++)
            {
                if (networkReceivers[i] == null)
                    continue;

                var pair = messageInvokers[networkReceivers[i]];

                if (pair.ContainsKey(message.Id))
                {
                    var invoker = pair[message.Id];
                    var packet = JObject.Parse(message.Message)?.ToObject(invoker.PacketType);

                    invoker.Method.Invoke(invoker.MessagerReciever, new object[] { packet });
                    ++invoked;
                }
            }

            if (invoked == 0)
            {
                D.Warn("There is no event handler : " + message.Id);
            }
        }
    }
}