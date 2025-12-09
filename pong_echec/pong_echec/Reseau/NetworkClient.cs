using System.Net.Sockets;
using System.Text;

namespace pong_echec.Reseau
{
    public class NetworkClient
    {
        TcpClient client;
        NetworkStream stream;

        public void Connect()
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 5000);
            stream = client.GetStream();
        }

        public void Send(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);
        }
        
        public string Receive()
        {
            byte[] buffer = new byte[1024];
            int len = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, len);
        }
    }
}
