using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TabletDriver
{
    public class Driver
    {
        public readonly dynamic config;
        public readonly Tuple<int, int> source;
        public readonly Tuple<int, int> target;
        public List<Filter> filters;

        public Driver(dynamic config) {
            this.config = config;
            this.filters = new List<Filter>();
            if (config.ContainsKey("SourceWidth") && config.ContainsKey("SourceHeight")) {
                source = Tuple.Create((int)config.SourceWidth, (int)config.SourceHeight);
            } else {
                source = Tuple.Create(320, 240);
            }
            if (config.ContainsKey("TargetWidth") && config.ContainsKey("TargetHeight")) {
                target = Tuple.Create((int)config.TargetWidth, (int)config.TargetHeight);
            } else {
                target = Tuple.Create(1920, 1080);
            }
            load_filters();
        }

        private void load_filters() {
            var antichatter = new AntiChatterFilter();
            if (antichatter.Load(config)) {
                filters.Add(antichatter);
            }
            var smoothing = new SmoothingFilter();
            if (smoothing.Load(config)) {
                filters.Add(smoothing);
            }
            var noise = new NoiseFilter();
            if (noise.Load(config)) {
                filters.Add(noise);
            }
            var stats = new StatsFilter();
            if (stats.Load(config)) {
                filters.Add(stats);
            }
        }

        public void HandleInput(int x, int y) {
            if (config.ContainsKey("percentage_x")) {
                x = (int)(x / (float)config.percentage_x);
            }
            if (config.ContainsKey("percentage_y")) {
                y = (int)(y / (float)config.percentage_y);
            }
            x = (int)(x * (double)(target.Item1 / source.Item1));
            y = (int)(y * (double)(target.Item2 / source.Item2));
            foreach (Filter filter in filters) {
                if (!filter.EditInput(ref x, ref y)) { return; }
            }
            Win32.POINT p = new Win32.POINT(x, y);
            Win32.ClientToScreen(Win32.GetDesktopWindow(), ref p);
            Win32.SetCursorPos(p.x, p.y);
        }

        public static void Main(string[] args) {
            dynamic config = JsonConvert.DeserializeObject(File.ReadAllText("config.json"));
            var instance = new Driver(config);
            var server_instance = new Server(8869, instance.HandleInput);
            server_instance.Listen();
        }


    }
}
