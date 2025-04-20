
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Minimoo.Common.Enum;
using Minimoo.Packet;
using Minimoo.Network.Encrypter;
using Minimoo.Network.Exceptions;
using Minimoo.Error;

namespace Minimoo.Network
{
    /// <summary>
    /// 미니무 네트워크 서비스 http 통신 담당
    /// </summary>
    public class NetworkService : Singleton<NetworkService>
    {
        [field: SerializeField]
        public ServerEnvironmentType Environment { get; set; } = ServerEnvironmentType.Production;

        [field: SerializeField]
        public string SessionKey { get; set; } = string.Empty;

        [field: SerializeField]
        public SocialType SocialType { get; set; } = SocialType.None;

        [field: SerializeField]
        public string SocialId { get; set; } = string.Empty;

        [field: SerializeField]
        public long AccountIdx { get; set; } = 0;

        [field: SerializeField]
        public GameIdType GameId { get; set; } = GameIdType.None;
        [field: SerializeField]
        public string GameVersion { get; set; } = string.Empty;

        #region Times

        [field: SerializeField]
        public double LoginSecondSinceStartUp { get; set; } = 0d;
        public DateTime LoginTime { get; set; } = DateTimeOffset.UtcNow.DateTime;

        private readonly PacketEncrypter encrypter = new();

        public int TIMEOUT_SECOND => 10 * 60;
        public long HEART_BEAT_TIME_MS => 1 * 60 * 1000;

        private long _lastHeartbeatTime = 0;

        public static Action<BasePacketResult> HEART_BEAT_RESULT_RECEIVED;

        #endregion


        public DateTime TimeNow
        {
            get
            {
                var timeSpanSecond = Time.realtimeSinceStartupAsDouble - LoginSecondSinceStartUp;
                return LoginTime.AddSeconds(timeSpanSecond);
            }
        }

        public long NowUnixMillisecond
        {
            get
            {
                return (TimeNow.Ticks / TimeSpan.TicksPerMillisecond) - (DateTime.UnixEpoch.Ticks / TimeSpan.TicksPerMillisecond);
            }
        }

        public long NowUnixSecond
        {
            get
            {
                return (TimeNow.Ticks / TimeSpan.TicksPerSecond) - (DateTime.UnixEpoch.Ticks / TimeSpan.TicksPerSecond);
            }
        }

        public bool IsConnected
        {
            get
            {
                return string.IsNullOrEmpty(SessionKey) == false && string.IsNullOrEmpty(SocialId) == false;
            }
        }

        public void Update()
        {
            var nowMS = NowUnixMillisecond;

            if (IsConnected)
            {
                if (nowMS - _lastHeartbeatTime > HEART_BEAT_TIME_MS)
                {
                    _lastHeartbeatTime = nowMS;
                    SendHeartBeatAsync();
                }
            }
            else
            {
                _lastHeartbeatTime = nowMS;
            }
        }

        public async void SendHeartBeatAsync()
        {
            var res = await GetAsync<HeartBeatHttpRes>(new HeartBeatHttpReq());
            HEART_BEAT_RESULT_RECEIVED?.Invoke(res);
        }

        public async UniTask<T> PostAsync<T>(BasePacket packet, int retryCounter = 0) where T : BasePacketResult
        {
            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    var errorMessage = $"Application Internet Rechability :  {Application.internetReachability.ToString()}";
                    throw new ErrorCodeException(ErrorCode.SYSTEM_ERROR, errorMessage);
                }

                var serverInfo = ServerInfoService.Instance.Get(Environment, packet.ServerType);

                Uri targetUri = new Uri(new Uri(serverInfo.MakeURI(URIType.Http)), packet.URI);
                string packetToJson = packet.ToJson();

                if (serverInfo.IsEncrypted)
                {
                    var encryptedBody = new EncryptedMessage() { Payload = serverInfo.Encrypter.Encrypt(packet.ToJson()) };
                    packetToJson = encryptedBody.ToJson();
                }

                D.Log($"URL: {targetUri} Send : {packet.ToJson()}");

                UnityWebRequest webRequest = UnityWebRequest.Post(targetUri, packetToJson, "application/json");
                byte[] jsonToSend = new UTF8Encoding().GetBytes(packetToJson);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("SessionKey", SessionKey);
                webRequest.timeout = TIMEOUT_SECOND;

                var res = await webRequest.SendWebRequest();

                switch (res.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Error: " + res.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("HTTP Error: " + res.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        break;
                }

                var packetResult = res.downloadHandler.text;

                try
                {
                    var encryptedBody = JsonConvert.DeserializeObject<EncryptedMessage>(packetResult);
                    if (encryptedBody != null && !string.IsNullOrEmpty(encryptedBody.Payload))
                        packetResult = serverInfo.Encrypter.Decrypt(encryptedBody.Payload);
                }
                catch { }

                D.Log($"URL: {targetUri} Received : {packetResult} ");
                var result = JsonConvert.DeserializeObject<T>(packetResult);

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Error: {e}");

                var result = (T)Activator.CreateInstance(typeof(T));
                result.ErrorCode = ErrorCode.SYSTEM_ERROR;
                result.ErrorMessage = e.Message;
                result.ErrorStackTrace = e.StackTrace;
                return result;
            }
        }

        public async UniTask<T> GetAsync<T>(BasePacket packet, int retryCounter = 0) where T : BasePacketResult
        {
            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    var errorMessage = $"Application Internet Rechability :  {Application.internetReachability.ToString()}";
                    throw new ErrorCodeException(ErrorCode.SYSTEM_ERROR, errorMessage);
                }

                var serverInfo = ServerInfoService.Instance.Get(Environment, packet.ServerType);

                var subUriBuilder = new StringBuilder();
                subUriBuilder.Append(packet.URI);

                Dictionary<string, object> parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(packet.ToJson());

                int paramCount = 0;
                foreach (var pair in parameters)
                {
                    if (paramCount < 1)
                        subUriBuilder.Append('?');
                    else
                        subUriBuilder.Append('&');

                    subUriBuilder.Append(pair.Key);
                    subUriBuilder.Append('=');
                    subUriBuilder.Append(pair.Value);

                    paramCount++;
                }

                var targetUri = new Uri(new Uri(serverInfo.MakeURI(URIType.Http)), subUriBuilder.ToString());

                D.Log($"URL: {targetUri} Send : {packet.ToJson()}");

                var webRequest = UnityWebRequest.Get(targetUri);
                webRequest.SetRequestHeader("SessionKey", SessionKey);
                webRequest.timeout = TIMEOUT_SECOND;

                var res = await webRequest.SendWebRequest();

                bool isFailed = false;
                switch (res.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Error: " + res.error);
                        isFailed = true;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("HTTP Error: " + res.error);
                        isFailed = true;
                        break;
                    case UnityWebRequest.Result.Success:
                        isFailed = false;
                        break;
                }

                if (isFailed && retryCounter > 0)
                {
                    // try again
                    return await GetAsync<T>(packet, retryCounter - 1);
                }

                var packetResult = res.downloadHandler.text;

                try
                {
                    var encryptedBody = JsonConvert.DeserializeObject<EncryptedMessage>(packetResult);
                    if (encryptedBody != null && !string.IsNullOrEmpty(encryptedBody.Payload))
                        packetResult = serverInfo.Encrypter.Decrypt(encryptedBody.Payload);
                }
                catch { }

                D.Log($"URL: {targetUri} Received : {packetResult} ");
                var result = JsonConvert.DeserializeObject<T>(packetResult);

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Error: {e}");
                var result = (T)Activator.CreateInstance(typeof(T));
                result.ErrorCode = ErrorCode.SYSTEM_ERROR;
                result.ErrorMessage = e.Message;
                result.ErrorStackTrace = e.StackTrace;
                return result;
            }
        }
    }
}
