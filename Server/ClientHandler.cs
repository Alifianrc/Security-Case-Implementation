using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using Security_Class_Library;
using System.Security.Cryptography;


namespace Server
{
    class ClientHandler
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        RsaEncryption encryption;
        BinaryFormatter formatter;
        int id;

        public ClientHandler(TcpClient tcpClient, int id, RsaEncryption encryption)
        {
            Console.WriteLine("Preparing client-" + id);

            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            this.encryption = new RsaEncryption();
            this.encryption.SetPrivateKey(encryption.privateKey);
            this.encryption.SetPublicKey(encryption.publicKey);
            formatter = new BinaryFormatter();
            this.id = id;

            // Get client public key
            GetClientPublicKey();

            Console.WriteLine("Client-" + id + " Ready!");
        }

        private void ReceiveData()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = formatter.Deserialize(networkStream).ToString();
                    string[] splitData = receivedData.Split("<spt>");
                    string massage = string.Empty;

                    for (int i = 0; i < (splitData.Length - 1); i++)
                    {
                        //massage += encryption.DecryptServer(splitData[i]);
                    }

                    Console.WriteLine("Client-" + id + " : " + massage);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }
        private string GetReceiveData(RSAParameters key)
        {
            try
            {
                string receivedData = (string)formatter.Deserialize(networkStream);
                return encryption.Decrypt(receivedData, key);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Data Size : " + e.Message);
            }
            
            return null;
        }

        private void GetInputSendMassage()
        {
            while (tcpClient.Connected)
            {
                string message = Console.ReadLine();
                //SendMassage(encryption.EncryptServer(message));
            }
        }
        private void SendMassage(string data)
        {
            formatter.Serialize(networkStream, data);
        }

        private void GetClientPublicKey()
        {
            string key = GetReceiveData(encryption.privateKey);
            encryption.AddOtherPublicKey(key);
            Console.WriteLine("Client-" + id + " Public Key Accepted");
        }
    }
}
