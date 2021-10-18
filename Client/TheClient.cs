using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class TheClient
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        public TheClient()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, 3001);
            networkStream = tcpClient.GetStream();

            Console.WriteLine("Client Started");

            Thread receiveThread = new Thread(RecieveMessage);
            Thread sendThread = new Thread(GetInputSendMassage);
            sendThread.Start();
            receiveThread.Start();
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
                            string theData = Encoding.ASCII.GetString(byteReceived);

                            Console.WriteLine("Server : " + theData);
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
