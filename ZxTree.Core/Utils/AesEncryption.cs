using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ZxTree.Core.Utils
{
    public class AesEncryption : IDisposable
    {
        private readonly Aes _aes;
        private readonly ICryptoTransform _enc;
        private readonly ICryptoTransform _dec;

        public AesEncryption(string token, string iv)
        {
            var (k, i) = GenerateKeyIVBytes(token, iv);

            _aes = Aes.Create();

            _aes.KeySize = k.Length * 8;
            _aes.Key = k;
            _aes.IV = i;

            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;

            _enc = _aes.CreateEncryptor(_aes.Key, _aes.IV);
            _dec = _aes.CreateDecryptor(_aes.Key, _aes.IV);
        }

        private static (byte[], byte[]) GenerateKeyIVBytes(string token, string iv)
        {

            using (var hash = SHA256.Create())
            using (var sha1 = SHA1.Create())
            {

                var key = hash.ComputeHash(Encoding.UTF8.GetBytes(token + iv));
                var iv1 = sha1.ComputeHash(Encoding.UTF8.GetBytes(iv + token)).Take(16).ToArray();

                return (key, iv1);
            }

        }

        public string Encrypt(string raw)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(raw)));
        }

        public byte[] Encrypt(byte[] raw)
        {
            return _enc.TransformFinalBlock(raw, 0, raw.Length);
        }

        public string Decrypt(string encrypted)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            return _dec.TransformFinalBlock(encrypted, 0, encrypted.Length);
        }


        /// <summary>
        /// Provides a delegate to encrypt/decrypt the configuration file
        /// </summary>
        /// <param name="raw">Content to be encrypted/decrypted</param>
        /// <param name="direction">0: Decryption, 1: Encryption</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string SecretProvider(string raw, int direction)
        { 
            return direction switch
            {
                0 => Decrypt(raw),
                1 => Encrypt(raw),
                _ => Encrypt(raw)
            };
        }
        public void Dispose()
        {
            _dec.Dispose();
            _enc.Dispose();
            _aes.Dispose();
        }
    }
}