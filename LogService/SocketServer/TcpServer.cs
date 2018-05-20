using System;
using System.Net;
using System.Net.Sockets;

namespace LogService.SocketServer
{
    class SocketAcceptedEventHandler : EventArgs
    {
        public Socket acceptedSocket
        {
            get;
            private set;
        }

        public SocketAcceptedEventHandler(Socket socket)
        {
            acceptedSocket = socket;
        }
    }

    class TcpServer
    {
        public event EventHandler<SocketAcceptedEventHandler> SocketAccepted;

        private Socket serverSocket;
        private int port;
        private bool listening;

        public TcpServer(int _port)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            port = _port;
        }

        public void Start(IPAddress ip)
        {
            if (listening)
                return;

            serverSocket.Bind(new IPEndPoint(ip, port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            listening = true;
        }

        public void Stop()
        {
            if (!listening)
                return;

            serverSocket.Close();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listening = false;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = serverSocket.EndAccept(ar);

                SocketAccepted?.Invoke(this, new SocketAcceptedEventHandler(socket));

                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch
            {

            }
        }
    }
}