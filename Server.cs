using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace TabletDriver
{
    public class Server
    {
        public readonly int port;
        public Action<int, int> callback;

        public Server(int port, Action<int, int> callback) { 
            this.port = port;
            this.callback = callback;
        }

        public void Listen() {
            UdpClient udpServer = new UdpClient(port);

            while (true) {
                var remoteEP = new IPEndPoint(IPAddress.Any, port);
                var data = udpServer.Receive(ref remoteEP);
                if (data.Length != 4) {
                    Console.WriteLine($"Received invalid data {data.Length}");
                    continue;
                }
                callback(CastBytes(data.Take(2).ToArray()), CastBytes(data.Skip(2).ToArray()));
            }
        }
        private int CastBytes(byte[] bytes) {
            if (bytes == null) {
                return 0;
            }
            return BitConverter.ToInt16(bytes, 0);
        }
    }
}
