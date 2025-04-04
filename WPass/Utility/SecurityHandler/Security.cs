﻿using System.IO;
using System.Security.Cryptography;
using System.Text;
using WPass.Core;

namespace WPass.Utility.SecurityHandler
{
    public class Security
    {
#if DEBUG
        private static readonly string EncryptionKey = Environment.GetEnvironmentVariable("MY_ENCRYPTION_KEY") ?? throw new InvalidOperationException("Encryption key is not set in the environment variables.");
#else
        private static readonly string EncryptionKey = DpapiHelper.LoadPassword("en_key.dat") ?? throw new InvalidOperationException("Encryption key file is not found.");
#endif

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.Mode = CipherMode.CBC;

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream ms = new(cipher);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }

        public static bool AreTheSame(string encrypt1, string encrypt2)
        {
            StringBuilder? sb1 = new();
            StringBuilder? sb2 = new();

            sb1.Append(Decrypt(encrypt1));
            sb2.Append(Decrypt(encrypt2));

            if (sb1.ToString().Equals(sb2.ToString()))
            {
                sb1.Clear();
                sb2.Clear();
                return true;
            }

            return false;
        }
    }
}
