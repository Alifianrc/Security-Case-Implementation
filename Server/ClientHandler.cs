using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace Server
{
    class ClientHandler
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        int id;

        public ClientHandler(TcpClient tcpClient, int id)
        {
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();

            this.id = id;

            Thread recieveData = new Thread(ReceiveData);
            Thread sendThread = new Thread(GetInputSendMassage);
            sendThread.Start();
            recieveData.Start();
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
                        string theData = Encoding.ASCII.GetString(byteReceived);

                        Console.WriteLine("Client-" + id + " : " + theData);
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

        private void GetInputSendMassage()
        {
            while (tcpClient.Connected)
            {
                string message = Console.ReadLine();
                SendMassage(message);
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
    }
}
