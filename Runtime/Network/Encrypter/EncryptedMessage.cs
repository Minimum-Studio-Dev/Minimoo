using Newtonsoft.Json;

namespace Minimoo.Network.Encrypter
{
    [System.Serializable]
    public sealed class EncryptedMessage
    {
        public string Payload { get; set; } = string.Empty;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
