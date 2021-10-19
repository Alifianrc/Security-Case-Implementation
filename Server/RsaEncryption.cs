using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace Server
{
    class RsaEncryption
    {
        private static RSACryptoServiceProvider csp;
        private RSAParameters privateKey;
        private RSAParameters publicKey;

        private RSAParameters clientPublicKey;

        private string filePath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";

        public RsaEncryption()
        {
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(false);
            publicKey = csp.ExportParameters(true);

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
            StreamReader reader = new StreamReader(key);
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            clientPublicKey = (RSAParameters)xs.Deserialize(reader);

            Console.WriteLine("Client public key accepted");
        }
    }
}
