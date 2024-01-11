using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


struct Config
{
    public double OG_RES_X = 320;
    public double OG_RES_Y = 240;
    public double TARGET_RES_X = 1920;
    public double TARGET_RES_Y = 1080;
    public double AREA_PERCENTAGE_X = 1.0;
    public double AREA_PERCENTAGE_Y = 1.0;
    public bool ANTI_CHATTER = false;
    public double ANTI_CHATTER_STRENGTH = 2;
    public bool SMOOTHING = false;
    public Config() {

    }

}

class TabletDriver
{
    public Config config;
    public double last_accepted_x = 0;
    public double last_accepted_y = 0;
    
    public TabletDriver(Config config) {
        this.config = config;
    }

    public static void Main() {
        var config = LoadConfig();
        var driver = new TabletDriver(config);
        driver.Listen();
    }

    static int CastBytes(byte[] bytes) {
        if (bytes == null) {
            return 0;
        }
        Console.WriteLine(bytes.Length);
        return BitConverter.ToInt16(bytes, 0);
    }

    static void MoveCursor(int x, int y) {
        Win32.POINT p = new Win32.POINT(x, y);
        Win32.ClientToScreen(Win32.GetDesktopWindow(), ref p);
        Win32.SetCursorPos(p.x, p.y);
    }

    public void HandleInput(byte[] bytes) {
        double x = CastBytes(bytes.Take(2).ToArray()) / config.AREA_PERCENTAGE_X;
        double y = CastBytes(bytes.Skip(2).ToArray()) / config.AREA_PERCENTAGE_Y;
        Console.WriteLine($"Received X {x} Y {y}");
        if (config.ANTI_CHATTER) {
            double delta = Math.Abs(last_accepted_x - x) + Math.Abs(last_accepted_y - y);
            if (delta < config.ANTI_CHATTER_STRENGTH) {
                return;
            }
        }
        if (config.SMOOTHING) {
            x = (x + last_accepted_x) / 2;
            y = (y + last_accepted_y) / 2;
        }
        last_accepted_x = x;
        last_accepted_y = y;
        MoveCursor((int)(x * (config.TARGET_RES_X / config.OG_RES_X)), (int)(y * (config.TARGET_RES_Y / config.OG_RES_Y)));
    }

    public void Listen() {
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
    static Config LoadConfig() {
        Config config = new Config();
        var path = "config.ini";
        try {
            using (var sr = new StreamReader(path)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    var split = line.Split('=');
                    if (split.Length != 2) {
                        continue;
                    }
                    var key = split[0];
                    var value = split[1];
                    switch (key) {
                        case "OG_RES_X":
                            config.OG_RES_X = double.Parse(value);
                            break;
                        case "OG_RES_Y":
                            config.OG_RES_Y = double.Parse(value);
                            break;
                        case "TARGET_RES_X":
                            config.TARGET_RES_X = double.Parse(value);
                            break;
                        case "TARGET_RES_Y":
                            config.TARGET_RES_Y = double.Parse(value);
                            break;
                        case "AREA_PERCENTAGE_X":
                            config.AREA_PERCENTAGE_X = double.Parse(value);
                            break;
                        case "AREA_PERCENTAGE_Y":
                            config.AREA_PERCENTAGE_Y = double.Parse(value);
                            break;
                        case "ANTI_CHATTER":
                            config.ANTI_CHATTER = bool.Parse(value);
                            break;
                        case "ANTI_CHATTER_STRENGTH":
                            config.ANTI_CHATTER_STRENGTH = double.Parse(value);
                            break;
                        case "SMOOTHING":
                            config.SMOOTHING = bool.Parse(value);
                            break;
                    }
                }
            }
        } catch {
            Console.WriteLine("Can't load config!");
            return config;
        }
        return config;
    }

}

