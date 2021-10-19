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

        private string filePath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";

        public RsaEncryption()
        {
            // Generate new client key
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(false);
            publicKey = csp.ExportParameters(true);

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
            try
            {
                var rCsp = new RSACryptoServiceProvider();
                rCsp.ImportParameters(serverPublicKey);
                byte[] byteData = Encoding.Unicode.GetBytes(dataText);
                byte[] cypher = rCsp.Encrypt(byteData, false);
                return Convert.ToBase64String(cypher);
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
