using System;
using Minimoo.Common.Enum;
using Minimoo.Network.Encrypter;
using UnityEngine;

namespace Minimoo.Network
{
    [Serializable]
    public class ServerInfo
    {
        [field: SerializeField]
        public string BaseAddress { get; set; } = string.Empty;
        [field: SerializeField]
        public int Port { get; set; } = 0;
        [field: SerializeField]
        public bool IsSecured { get; set; } = false;
        [field: SerializeField]
        public bool IsEncrypted { get; set; } = false;

        [field: SerializeField]
        public string AesKey { get; set; } = string.Empty;

        [field: SerializeField]
        public string AesIV { get; set; } = string.Empty;

        private PacketEncrypter _encrypter = null;

        public PacketEncrypter Encrypter
        {
            get
            {
                if (_encrypter == null)
                {
                    _encrypter = new PacketEncrypter(AesKey, AesIV);
                }

                return _encrypter;
            }
        }

        public string MakeURI(URIType type)
        {
            switch (type)
            {
                case URIType.Http:
                default:
                    return $"{(IsSecured == false ? "http://" : "https://")}{BaseAddress}:{Port}";

                case URIType.WebSocket:
                    return $"{(IsSecured == false ? "ws://" : "wss://")}{BaseAddress}:{Port}/ws";
            }
        }
    }

}
