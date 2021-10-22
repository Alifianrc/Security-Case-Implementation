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

        static byte[] dataBuffer = new byte[4096];

        public ClientHandler(TcpClient tcpClient, int id, RsaEncryption encryption)
        {
            Console.WriteLine("Preparing client-" + id);

            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            this.rsaEncryption = new RsaEncryption();
            this.rsaEncryption.SetPrivateKey(encryption.privateKey);
            this.rsaEncryption.SetPublicKey(encryption.publicKey);
            aesEncryption = new AesEncryption();
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
                byte[] byteSizeData = new byte[300];
                networkStream.Read(byteSizeData, 0, byteSizeData.Length);
                int dataSize = BitConverter.ToInt32(byteSizeData);

                byte[] encryptByte = new byte[dataSize];
                networkStream.Read(encryptByte, 0, encryptByte.Length);
                string encryptedData = Encoding.ASCII.GetString(encryptByte);

                string decryptedData = rsaEncryption.Decrypt(encryptedData, key);

                Console.WriteLine("Client-" + id + " : " + decryptedData);
            }
        }
        private void ReceiveData(AesCryptoServiceProvider key)
        {
            while (tcpClient.Connected)
            {
                byte[] byteSizeData = new byte[300];
                networkStream.Read(byteSizeData, 0, byteSizeData.Length);
                int dataSize = BitConverter.ToInt32(byteSizeData);

                byte[] encryptByte = new byte[dataSize];
                networkStream.Read(encryptByte, 0, encryptByte.Length);
                string encryptedData = Encoding.ASCII.GetString(encryptByte);

                string decryptedData = aesEncryption.Decrypt(encryptedData, key);

                Console.WriteLine("Client-" + id + " : " + decryptedData);
            }
        }
        private string GetReceiveData(RSAParameters key)
        {
            byte[] byteSizeData = new byte[300];
            networkStream.Read(byteSizeData, 0, byteSizeData.Length);
            int dataSize = BitConverter.ToInt32(byteSizeData);

            byte[] encryptByte = new byte[dataSize];
            networkStream.Read(encryptByte, 0, encryptByte.Length);
            string encryptedData = Encoding.ASCII.GetString(encryptByte);

            return rsaEncryption.Decrypt(encryptedData, key);
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
            string encryptData = rsaEncryption.Encrypt(data, key);
            byte[] byteData = Encoding.ASCII.GetBytes(encryptData);

            int dataLength = byteData.Length;
            byte[] byteDataLength = BitConverter.GetBytes(dataLength);

            networkStream.Write(byteDataLength, 0, byteDataLength.Length);
            Thread.Sleep(50);
            networkStream.Write(byteData, 0, byteData.Length);
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
            string encryptData = aesEncryption.Encrypt(data, key);
            byte[] byteData = Encoding.ASCII.GetBytes(encryptData);

            int dataLength = byteData.Length;
            byte[] byteDataLength = BitConverter.GetBytes(dataLength);

            networkStream.Write(byteDataLength, 0, byteDataLength.Length);
            Thread.Sleep(50);
            networkStream.Write(byteData, 0, byteData.Length);
        }

        private void PrepareClient()
        {
            // Get client public key
            GetClientPublicKey();
            // Make a new Symmetric key
            aesEncryption.GenerateNewKey();
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
