using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Security_Class_Library
{
    public class RsaEncryption
    {
        private static RSACryptoServiceProvider csp;
        public RSAParameters privateKey { get; private set; }
        public RSAParameters publicKey { get; private set; }
        public List<RSAParameters> listOtherPublicKey { get; private set; }
        public string txtPath { get; private set; }

        private static int MaxEncryptSize = 100;

        public RsaEncryption()
        {
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(true);
            publicKey = csp.ExportParameters(false);
            listOtherPublicKey = new List<RSAParameters>();
            txtPath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";
        }

        // ------------------------------------ General Method ---------------------------------------------------------
        public string Encrypt(string dataText, RSAParameters key)
        {
            if (key.Equals(null))
            {
                Console.WriteLine("Server public key not found");
                return null;
            }

            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(key);
                byte[] byteData = Encoding.Unicode.GetBytes(dataText);
                int readPos = 0;
                string encryptedData = string.Empty;

                while (byteData.Length - readPos > 0)
                {
                    byte[] splitToEncrypt = new byte[MaxEncryptSize];

                    if (byteData.Length - (readPos + MaxEncryptSize) > 0)
                    {
                        Array.Copy(byteData, readPos, splitToEncrypt, 0, 100);
                        readPos += MaxEncryptSize;
                    }
                    else
                    {
                        Array.Copy(byteData, readPos, splitToEncrypt, 0, byteData.Length - readPos);
                        readPos += byteData.Length - readPos;
                    }

                    byte[] encryptedByte = rCsp.Encrypt(splitToEncrypt, false);
                    encryptedData += Convert.ToBase64String(encryptedByte);
                    encryptedData += "<spt>";
                }
                
                return encryptedData;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error encrypt server : " + e.Message);
            }

            return null;
        }
        public string Decrypt(string dataCypher, RSAParameters key)
        {
            if (key.Equals(null))
            {
                Console.WriteLine("Empty private key");
                return null;
            }

            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(key);
                string[] splitData = dataCypher.Split("<spt>");
                string dataDecrypted = string.Empty;

                for(int i = 0; i < splitData.Length - 1; i++)
                {
                    byte[] dataByte = Convert.FromBase64String(splitData[i]);
                    byte[] dataPlain = rCsp.Decrypt(dataByte, false);
                    dataDecrypted += Encoding.Unicode.GetString(dataPlain);
                }
                
                return dataDecrypted;
            }
            catch (Exception e)
            {
                Console.WriteLine("Decryption server error : " + e.Message);
            }

            return null;
        }

        public string ConvertKeyToString(RSAParameters key)
        {
            try
            {
                StringWriter sw = new StringWriter();
                XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, key);
                return sw.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Key To String : " + e.Message);
            }
            return null;
        }
        public RSAParameters ConvertStringToKey(String key)
        {
            try
            {
                StringReader reader = new StringReader(key);
                XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
                RSAParameters theKey = (RSAParameters)xs.Deserialize(reader);

                return theKey;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error String To Key : " + e.Message);
            }

            RSAParameters emptyKey = new RSAParameters();
            return emptyKey;
        }

        public void AddOtherPublicKey(string key)
        {
            listOtherPublicKey.Add(ConvertStringToKey(key));
        }
        public void AddOtherPublicKey(RSAParameters key)
        {
            listOtherPublicKey.Add(key);
        }

        public void SetPrivateKey(RSAParameters key)
        {
            privateKey = key;
        }
        public void SetPublicKey(RSAParameters key)
        {
            publicKey = key;
        }

        // ------------------------------------ Specific Method ---------------------------------------------------------
        public void SaveKey(RSAParameters key, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(ConvertKeyToString(key));
            writer.Close();
        }
        public void SaveKey(string key, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(key);
            writer.Close();
        }

        public RSAParameters LoadKey(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            RSAParameters key = (RSAParameters)xs.Deserialize(reader);
            return key;
        }
        public string LoadKeyInString(string filePath)
        {
            string key = File.ReadAllText(filePath);
            return key;
        }

        public bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }
    }
}
