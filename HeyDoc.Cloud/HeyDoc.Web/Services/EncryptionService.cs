using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class EncryptionService
    {
        public static byte[] EncryptAES(byte[] key, byte[] plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // Prepend IV to ciphertext in the MemoryStream
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var bwEncrypt = new BinaryWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            bwEncrypt.Write(plainText);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        public static byte[] DecryptAES(byte[] key, byte[] cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                var iv = new byte[aes.IV.Length];
                Buffer.BlockCopy(cipherText, 0, iv, 0, aes.IV.Length);

                var decryptor = aes.CreateDecryptor(aes.Key, iv);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        using (var bwDecrypt = new BinaryWriter(csDecrypt))
                        {
                            // Write the encrypted data to the stream. msDecrypt will then contain the decrypted data
                            bwDecrypt.Write(cipherText, aes.IV.Length, cipherText.Length - aes.IV.Length);
                        }
                    }
                    return msDecrypt.ToArray();
                }
            }
        }

        public static byte[] EncryptStringAES(byte[] key, string plainText)
        {
            return EncryptAES(key, Encoding.UTF8.GetBytes(plainText));
        }

        public static string DecryptStringAES(byte[] key, byte[] cipherText)
        {
            return Encoding.UTF8.GetString(DecryptAES(key, cipherText));
        }
    }
}
