using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;


namespace Server
{
    class ClientHandler
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        RsaEncryption encryption;

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
                            Console.WriteLine("Client-" + id + " : " + theData);
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
                catch(Exception e)
                {
                    Console.WriteLine("Error Data Size");
                    break;
                }                
            }
        }
        private string GetReceiveData()
        {
            int dataSize = 0; 

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
                    string theData = Convert.ToBase64String(byteReceived);

                    return theData;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Data");
                }
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
            string mSize = Encoding.ASCII.GetByteCount(data).ToString();
            byte[] sizeSend = Encoding.ASCII.GetBytes(mSize);
            networkStream.Write(sizeSend, 0, sizeSend.Length);

            byte[] byteData = Encoding.ASCII.GetBytes(data);
            networkStream.Write(byteData, 0, byteData.Length);
        }

        private void GetClientPublicKey()
        {
            string encryptedData = GetReceiveData(); Console.WriteLine(encryptedData);
            string data = encryption.DecryptServer(encryptedData);
            Console.WriteLine(data);
            //encryption.SetClientPublicKey(key);

            Console.WriteLine("Client public key accepted");
        }
    }
}
