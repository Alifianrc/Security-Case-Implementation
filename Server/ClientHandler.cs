using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;


namespace Server
{
    class ClientHandler
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        RsaEncryption encryption;

        BinaryFormatter formatter = new BinaryFormatter();

        int id;

        public ClientHandler(TcpClient tcpClient, int id, RsaEncryption encryption)
        {
            Console.WriteLine("Preparing client-" + id);

            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            this.id = id;
            this.encryption = encryption;

            // Get client public key
            GetClientPublicKey();

            // Start normal data transfer
            //Thread recieveData = new Thread(ReceiveData);
            //Thread sendThread = new Thread(GetInputSendMassage);
            //sendThread.Start();
            //recieveData.Start();

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
                        massage += encryption.DecryptServer(splitData[i]);
                    }

                    Console.WriteLine("Client-" + id + " : " + massage);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }
        private string GetReceiveData()
        {
            try
            {
                string receivedData = (string)formatter.Deserialize(networkStream);
                return receivedData;
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
                SendMassage(encryption.EncryptServer(message));
            }
        }
        private void SendMassage(string data)
        {
            formatter.Serialize(networkStream, data);
        }

        private void GetClientPublicKey()
        {
            string encryptedData = GetReceiveData();
            string[] splitEncryptedData = encryptedData.Split("<spt>");

            string key = string.Empty;

            for (int i = 0; i < splitEncryptedData.Length - 1; i++)
            {
                key += encryption.DecryptServer(splitEncryptedData[i]);
            }

            encryption.SetClientPublicKey(key);
        }
    }
}
