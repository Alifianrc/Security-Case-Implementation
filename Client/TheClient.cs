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

        public TheClient()
        {
            Console.WriteLine("Preparing client");

            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, 3001);
            networkStream = tcpClient.GetStream();

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
                int dataSize = 0;

                while (tcpClient.Connected)
                {
                    try
                    {
                        byte[] byteSizeReceived = new byte[257];
                        networkStream.Read(byteSizeReceived, 0, 257);
                        string dS = Encoding.ASCII.GetString(byteSizeReceived);
                        dataSize = int.Parse(dS);

                        try
                        {
                            byte[] byteReceived = new byte[dataSize];
                            networkStream.Read(byteReceived, 0, dataSize);
                            string encryptedData = Encoding.ASCII.GetString(byteReceived);
                            try
                            {
                                string theData = encryption.DecryptServer(encryptedData);
                                Console.WriteLine("Server : " + theData);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error Decryption : " + e.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error Data");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Data Size");
                        break;
                    }
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
            string mSize = Encoding.ASCII.GetByteCount(data).ToString();
            byte[] sizeSend = Encoding.ASCII.GetBytes(mSize);
            networkStream.Write(sizeSend, 0, sizeSend.Length);

            byte[] byteData = Convert.FromBase64String(EncodeTo64(data));
            networkStream.Write(byteData, 0, byteData.Length);
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
            Console.WriteLine("Sending public key to server");

            string key = encryption.GetPublicKey();
            string send = encryption.EncryptServer(key); Console.WriteLine(send);
            SendMassage(send);       
        }
    }
}
