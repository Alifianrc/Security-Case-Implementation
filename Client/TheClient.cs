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
        RsaEncryption rsaEncryption;
        AesEncryption aesEncryption;
        BinaryFormatter formatter;

        public TheClient()
        {
            Console.WriteLine("Preparing client");

            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, 3001);
            networkStream = tcpClient.GetStream();
            rsaEncryption = new RsaEncryption();
            aesEncryption = new AesEncryption();
            formatter = new BinaryFormatter();

            // Client preparation
            Thread preparation = new Thread(PrepareClient);
            preparation.Start();
            preparation.Join();

            // Start normal data transfer
            // Using symmetric key
            Thread receiveThread = new Thread(() => RecieveMessage(aesEncryption.aes));
            Thread sendThread = new Thread(() => GetInputSendMassage(aesEncryption.aes));
            sendThread.Start();
            receiveThread.Start();

            Console.WriteLine("Client Ready");
        }

        private void RecieveMessage(RSAParameters key)
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = (string)formatter.Deserialize(networkStream);
                    Console.WriteLine("Server : " + rsaEncryption.Decrypt(receivedData, key));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }
        private void RecieveMessage(AesCryptoServiceProvider key)
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = (string)formatter.Deserialize(networkStream);
                    Console.WriteLine("Server : " + aesEncryption.Decrypt(receivedData, key));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }
        private string GetReceiveData(RSAParameters key)
        {
            string receivedData = formatter.Deserialize(networkStream).ToString();
            return rsaEncryption.Decrypt(receivedData, key);
        }

        private void GetInputSendMassage(RSAParameters key)
        {
            while (tcpClient.Connected)
            {
                string massage = Console.ReadLine();
                SendMassage(massage, key);
            }
        }
        private void SendMassage(string data, RSAParameters key)
        {
            formatter.Serialize(networkStream, rsaEncryption.Encrypt(data, key));
        }
        private void GetInputSendMassage(AesCryptoServiceProvider key)
        {
            while (tcpClient.Connected)
            {
                string massage = Console.ReadLine();
                SendMassage(massage, key);
            }
        }
        private void SendMassage(string data, AesCryptoServiceProvider key)
        {
            formatter.Serialize(networkStream, aesEncryption.Encrypt(data, key));
        }

        private void PrepareClient()
        {
            // Load server public key
            LoadServerPublicKey();
            // Send our public key to server
            SendPublicKeyToServer();
            // Accept new symmetic key from server
            AcceptServerSymmetricKey();
        }
        private void LoadServerPublicKey()
        {
            rsaEncryption.AddOtherPublicKey(rsaEncryption.LoadKey(rsaEncryption.txtPath));
        }
        private void SendPublicKeyToServer()
        {
            // Send client public key (encrypted by Server public key)
            Console.WriteLine("Sending public key to server ...");
            string key = rsaEncryption.ConvertKeyToString(rsaEncryption.publicKey);
            SendMassage(key, rsaEncryption.listOtherPublicKey[0]);              
        }
        private void AcceptServerSymmetricKey()
        {
            Console.WriteLine("Server Symmetric key accepted!");
            string key = GetReceiveData(rsaEncryption.privateKey);
            aesEncryption.SetKey(aesEncryption.ConvertStringToKey(key));
        }

        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }
    }
}
