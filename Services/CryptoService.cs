using System;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
    public class CryptoService
    {
        private const int SaltSize = 16;
        private const int IvSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public string Encrypt(string plainText, string passphrase)
        {
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
                rng.GetBytes(iv);
            }

            var key = DeriveKey(passphrase, salt);

            byte[] cipherText;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            var result = new byte[SaltSize + IvSize + cipherText.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
            Buffer.BlockCopy(cipherText, 0, result, SaltSize + IvSize, cipherText.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string base64, string passphrase)
        {
            var data = Convert.FromBase64String(base64);

            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            var cipherText = new byte[data.Length - SaltSize - IvSize];

            Buffer.BlockCopy(data, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(data, SaltSize, iv, 0, IvSize);
            Buffer.BlockCopy(data, SaltSize + IvSize, cipherText, 0, cipherText.Length);

            var key = DeriveKey(passphrase, salt);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        private static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }
    }
}
