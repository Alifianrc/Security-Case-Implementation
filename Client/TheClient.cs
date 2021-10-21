using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Security_Class_Library;
using System.Security.Cryptography;

namespace Client
{
    class TheClient
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        RsaEncryption encryption;
        BinaryFormatter formatter;

        public TheClient()
        {
            Console.WriteLine("Preparing client");

            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, 3001);
            networkStream = tcpClient.GetStream();
            encryption = new RsaEncryption();
            formatter = new BinaryFormatter();

            // Load server public key
            LoadServerPublicKey();
            // Send our public key to server
            SendPublicKeyToServer();

            // Start normal data transfer
            Thread receiveThread = new Thread(RecieveMessage);
            //Thread sendThread = new Thread(GetInputSendMassage);
            //sendThread.Start();
            receiveThread.Start();

            Console.WriteLine("Client Ready");
        }

        private void RecieveMessage()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = (string)formatter.Deserialize(networkStream);
                    //Console.WriteLine("Server : " + encryption.DecryptServer(receivedData));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }

        private void GetInputSendMassage()
        {
            while (tcpClient.Connected)
            {
                string message = Console.ReadLine();
                //SendMassage(encryption.EncryptServer(message));
            }
        }
        private void SendMassage(string data, RSAParameters key)
        {
            formatter.Serialize(networkStream, encryption.Encrypt(data, key));
        }

        private void LoadServerPublicKey()
        {
            encryption.AddOtherPublicKey(encryption.LoadKey(encryption.txtPath));
        }
        private void SendPublicKeyToServer()
        {
            // Send client public key (encrypted by Server public key)
            Console.WriteLine("Sending public key to server ...");
            string key = encryption.ConvertKeyToString(encryption.publicKey);
            SendMassage(key, encryption.listOtherPublicKey[0]);              
        }
    }
}
