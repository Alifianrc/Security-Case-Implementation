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
        RsaEncryption rsaEncryption;
        AesEncryption aesEncryption;
        BinaryFormatter formatter;
        int id;

        public ClientHandler(TcpClient tcpClient, int id, RsaEncryption encryption)
        {
            Console.WriteLine("Preparing client-" + id);

            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            this.rsaEncryption = new RsaEncryption();
            this.rsaEncryption.SetPrivateKey(encryption.privateKey);
            this.rsaEncryption.SetPublicKey(encryption.publicKey);
            formatter = new BinaryFormatter();
            this.id = id;

            // Client preparation
            Thread preparation = new Thread(PrepareClient);
            preparation.Start();
            preparation.Join();

            // Start normal data transfer
            // Using symmetric key
            Thread receiveThread = new Thread(() => ReceiveData(aesEncryption.aes));
            Thread sendThread = new Thread(() => GetInputSendMassage(aesEncryption.aes));
            sendThread.Start();
            receiveThread.Start();

            Console.WriteLine("Client-" + id + " Ready!");
        }

        private void ReceiveData(RSAParameters key)
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = (string)formatter.Deserialize(networkStream);
                    Console.WriteLine("Client-" + id + " : " + rsaEncryption.Decrypt(receivedData, key));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data Size : " + e.Message);
                }
            }
        }
        private void ReceiveData(AesCryptoServiceProvider key)
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string receivedData = (string)formatter.Deserialize(networkStream);
                    Console.WriteLine("Client-" + id + " : " + aesEncryption.Decrypt(receivedData, key));
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
                return rsaEncryption.Decrypt(receivedData, key);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Data Size : " + e.Message);
            }
            
            return null;
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
            // Get client public key
            GetClientPublicKey();
            // Make a new Symmetric key
            aesEncryption = new AesEncryption();
            // Send the symmetric key
            SendSymmetricKey();
        }
        private void GetClientPublicKey()
        {
            string key = GetReceiveData(rsaEncryption.privateKey);
            rsaEncryption.AddOtherPublicKey(key);
            Console.WriteLine("Client-" + id + " Public Key Accepted");
        }
        private void SendSymmetricKey()
        {
            string key = aesEncryption.ConvertKeyToString(aesEncryption.aes);
            SendMassage(key, rsaEncryption.listOtherPublicKey[0]);
            Console.WriteLine("Sending Symmetric Key to " + "Client-" + id + " ...");
        }
    }
}
