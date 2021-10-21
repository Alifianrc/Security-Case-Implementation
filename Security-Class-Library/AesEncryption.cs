using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Security_Class_Library
{
    public class AesEncryption
    {
        public AesCryptoServiceProvider aes { get; private set; }

        public AesEncryption()
        {
            aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.GenerateIV();
            aes.GenerateKey();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
        }

        public string Encrypt(string data, AesCryptoServiceProvider key)
        {
            try
            {
                ICryptoTransform transform = key.CreateEncryptor();
                byte[] encryptedByte = transform.TransformFinalBlock(Encoding.Unicode.GetBytes(data), 0, data.Length);

                return Convert.ToBase64String(encryptedByte);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Aes encrypt : " + e.Message);
            }

            return null;
        }
        public string Decrypt(string data, AesCryptoServiceProvider key)
        {
            try
            {
                ICryptoTransform transform = key.CreateDecryptor();
                byte[] decryptedByte = transform.TransformFinalBlock(Convert.FromBase64String(data), 0, data.Length);

                return Convert.ToBase64String(decryptedByte);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Aes decrypt : " + e.Message);
            }

            return null;
        }

        public string ConvertKeyToString(AesCryptoServiceProvider key)
        {
            string theKey = Convert.ToBase64String(key.Key); Console.WriteLine("Origin :" + theKey);
            return theKey;
        }
        public AesCryptoServiceProvider ConvertStringToKey(string key)
        {
            AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider(); Console.WriteLine("New :" + key);
            aesCrypto.Key = Convert.FromBase64String(key);
            return aesCrypto;
        }

        public void SetKey(AesCryptoServiceProvider aesCrypto)
        {
            aes = aesCrypto;
        }


        public bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }
    }
}
