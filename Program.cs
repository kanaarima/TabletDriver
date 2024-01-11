using System.Net;
using System.Net.Sockets;

const float OG_RES_X = 320;
const float OG_RES_Y = 240;
const float TARGET_RES_X = 1920;
const float TARGET_RES_Y = 1080;

static int CastBytes(byte[] bytes) {
    if (bytes == null) {
        return 0;
    }
    Console.WriteLine(bytes.Length);
    return BitConverter.ToInt16(bytes, 0);
}

static void MoveCursor(int x,  int y) {
    Win32.POINT p = new Win32.POINT(x, y);
    Win32.ClientToScreen(Win32.GetDesktopWindow(), ref p);
    Win32.SetCursorPos(p.x, p.y);
}

static void HandleInput(byte[] bytes) {
    int x = CastBytes(bytes.Take(2).ToArray());
    int y = CastBytes(bytes.Skip(2).ToArray());

    Console.WriteLine($"Received X {x} Y {y}");
    MoveCursor((int)(x * (TARGET_RES_X/OG_RES_X)), (int)(y * (TARGET_RES_Y / OG_RES_Y)));
}

static void Listen() {
    UdpClient udpServer = new UdpClient(8869);

    while (true) {
        var remoteEP = new IPEndPoint(IPAddress.Any, 8869);
        var data = udpServer.Receive(ref remoteEP);
        if (data.Length != 4) {
            Console.WriteLine($"Received invalid data {data.Length}");
            continue;
        }
        HandleInput(data);
    }
}

Listen();