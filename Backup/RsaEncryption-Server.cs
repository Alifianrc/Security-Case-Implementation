using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Server
{
    class RsaEncryption
    {
        private static RSACryptoServiceProvider csp;
        private RSAParameters privateKey;
        private RSAParameters publicKey;

        private RSAParameters clientPublicKey;

        private static int MaxEncryptSize = 100;

        private string filePath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";

        public RsaEncryption()
        {
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(true);
            publicKey = csp.ExportParameters(false);

            SavePublicKey();
        }

        // Save public key to be accsess by client
        // (Client install simulation)
        private void SavePublicKey()
        {
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(GetPublicKey());
            writer.Close();
        }

        // How to get public key
        public string GetPublicKey()
        {
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, publicKey);
            return sw.ToString();
        } 

        // Encrypt server private key method
        public string EncryptServer(string dataText)
        {
            var rCsp = new RSACryptoServiceProvider(); 
            rCsp.ImportParameters(privateKey);
            byte[] byteData = Encoding.Unicode.GetBytes(dataText);
            byte[] cypher = rCsp.Encrypt(byteData, false);

            return Convert.ToBase64String(cypher);
        }
        // Decrypt server private key method
        public string DecryptServer(string dataCypher)
        {
            if (privateKey.Equals(null))
            {
                Console.WriteLine("Empty private key");
                return null;
            }

            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(privateKey);
                byte[] dataByte = Convert.FromBase64String(dataCypher); 
                byte[] dataPlain = rCsp.Decrypt(dataByte, false);
                
                return Encoding.Unicode.GetString(dataPlain);
            }
            catch (Exception e)
            {
                Console.WriteLine("Decryption server error : " + e.Message);
            }
            return null;
        }

        // Set client public key
        public void SetClientPublicKey(string key)
        {
            try
            {
                StringReader reader = new StringReader(key);
                XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
                clientPublicKey = (RSAParameters)xs.Deserialize(reader);

                Console.WriteLine("Client public key accepted!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Set Client Key : " + e.Message);
            }
        }

        // Encrpt data with client public key
        public string EncryptClient(string dataText)
        {
            if (clientPublicKey.Equals(null))
            {
                Console.WriteLine("Server public key not found");
                return null;
            }

            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(clientPublicKey);
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

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}
