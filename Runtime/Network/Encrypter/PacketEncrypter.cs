using System.Security.Cryptography;
using System.Text;

namespace Minimoo.Network.Encrypter
{

    public sealed class PacketEncrypter
    {
        public Aes Cipher { get; set; }

        public PacketEncrypter() : base() {}

        public PacketEncrypter(string key, string iv) : base()
        {
            var aes = Aes.Create();
            var bytesKey = Encoding.UTF8.GetBytes(key); // 32
            aes.Key = bytesKey;
            var ivKey = Encoding.UTF8.GetBytes(iv); // 16
            aes.IV = ivKey;
            Cipher = aes;
        }
    }
}