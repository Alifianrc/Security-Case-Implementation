using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Client
{
    class RsaEncryption
    {
        private static RSACryptoServiceProvider csp;
        private RSAParameters privateKey;
        private RSAParameters publicKey;

        private RSAParameters serverPublicKey;

        private static int MaxEncryptSize = 100;

        private string filePath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";

        public RsaEncryption()
        {
            // Generate new client key
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(true);
            publicKey = csp.ExportParameters(false);

            GetServerPublicKey();
        }

        // Get Server public key
        // Simulation client game installation
        public void GetServerPublicKey()
        {
            StreamReader reader = new StreamReader(filePath);
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            serverPublicKey = (RSAParameters)xs.Deserialize(reader);

            Console.WriteLine("Server public key insatalled");
        }

        // Decrypt server public key method
        public string DecryptServer(string dataCypher)
        {
            var rCsp = new RSACryptoServiceProvider();
            rCsp.ImportParameters(serverPublicKey);
            byte[] dataByte = Convert.FromBase64String(dataCypher);
            byte[] dataPlain = rCsp.Decrypt(dataByte, false);

            return Encoding.Unicode.GetString(dataPlain);
        }
        // Encrypt server public key method
        public string EncryptServer(string dataText)
        {
            if (serverPublicKey.Equals(null))
            {
                Console.WriteLine("Server public key not found");
                return null;
            }

            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(serverPublicKey);
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

        // How to get public key
        public string GetPublicKey()
        {
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, publicKey);
            return sw.ToString();
        }
    }
}
