using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
            formatter = new BinaryFormatter();

            // Install server public key inside here
            encryption = new RsaEncryption();

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
                    Console.WriteLine("Server : " + encryption.DecryptServer(receivedData));
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
                SendMassage(encryption.EncryptServer(message));
            }
        }
        private void SendMassage(string data)
        {
            formatter.Serialize(networkStream, data);
        }
        string EncodeTo64(string toEncode)

        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private void SendPublicKeyToServer()
        {
            // Send client public key (encrypted by Server public key)
            Console.WriteLine("Sending public key to server ...");

            string key = encryption.GetPublicKey();
            string send = encryption.EncryptServer(key); Console.WriteLine(key + "End");
            SendMassage(send);       
        }
    }
}
