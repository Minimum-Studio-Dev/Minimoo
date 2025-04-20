using System;
using System.IO;
using System.Security.Cryptography;

namespace Minimoo.Network.Encrypter
{
    public static class PacketEncrypterExtensions
    {
        public static string Encrypt(this PacketEncrypter encrypter, string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var cipher = encrypter.Cipher;
            ICryptoTransform encryptor = cipher.CreateEncryptor(cipher.Key, cipher.IV);
            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                //Write all data to the stream.
                swEncrypt.Write(data);
            }
            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(this PacketEncrypter encrypter, string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var cipher = encrypter.Cipher;
            ICryptoTransform decryptor = cipher.CreateDecryptor(cipher.Key, cipher.IV);

            using MemoryStream msDecrypt = new(Convert.FromBase64String(data));
            using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}