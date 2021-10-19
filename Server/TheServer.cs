using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class TheServer
    {
        private TcpListener TcpListener;

        private RsaEncryption encryption;

        private int clientCount;

        public TheServer()
        {
            encryption = new RsaEncryption();

            TcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 3001));
            TcpListener.Start();

            clientCount = 0;

            Console.WriteLine("Server Started");

            BeginListening();
        }

        private void BeginListening()
        {
            while (true)
            {
                // Accecpt new client
                clientCount++;
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                Console.WriteLine("Client No: " + Convert.ToString(clientCount) + " connected!");

                // Handle client massage
                ClientHandler clientHandler = new ClientHandler(tcpClient, clientCount, encryption);
            }
        }
    }
}
