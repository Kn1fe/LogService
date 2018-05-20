using System;
using System.Net;
using System.Net.Sockets;

namespace LogService.SocketServer
{
    class UdpServer : IDisposable
    {
        private UdpClient listener;
        private IPEndPoint serverEndPoint;

        public UdpServer(IPAddress ip, int port)
        {
            serverEndPoint = new IPEndPoint(ip, port);
            listener = new UdpClient(serverEndPoint);
            listener.BeginReceive(ReceivedDatagram, null);
        }

        public void SendData(byte[] buffer, IPEndPoint clientEndPoint)
        {
            try
            {
                listener.Send(buffer, buffer.Length, clientEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error (SendData): " + e);
            }
        }

        private void ReceivedDatagram(IAsyncResult ar)
        {
            byte[] buffer = listener.EndReceive(ar, ref serverEndPoint);
            Received?.Invoke(serverEndPoint, buffer);
            listener.BeginReceive(ReceivedDatagram, null);
        }

        public void Dispose()
        {
            listener.Close();
        }

        public delegate void UdpDatagramReceivedHandler(IPEndPoint sender, byte[] data);

        public event UdpDatagramReceivedHandler Received;
    }
}
