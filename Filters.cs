using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TabletDriver
{
    public abstract class Filter
    {
        public abstract bool EditInput(ref int x, ref int y);
        public abstract bool Load(dynamic config);
    }

    public class SmoothingFilter : Filter
    {

        private int strength = 1;
        private List<Tuple<int, int>> lastInputs = new List<Tuple<int, int>>();
        public override bool EditInput(ref int x, ref int y) {
            lastInputs.Add(Tuple.Create(x, y));
            if (lastInputs.Count > strength) {
                lastInputs.RemoveAt(0);
            }
            int allX = 0;
            int allY = 0;
            foreach (Tuple<int, int> input in lastInputs) {
                allX += input.Item1;
                allY += input.Item2;
            }
            x = allX / lastInputs.Count;
            y = allY / lastInputs.Count;
            return true;
        }
        public override bool Load(dynamic config) {
            if (!config.ContainsKey("smoothing")) {
                return false;
            }
            if (config.smoothing.ContainsKey("enabled")) {
                if (!(bool)config.smoothing.enabled) {
                    return false;
                }
            }
            if (config.smoothing.ContainsKey("strength")) {
                strength = (int)config.smoothing.strength;
            }
            return true;
        }

    }

    public class AntiChatterFilter : Filter
    {

        private int strength = 2;
        private Tuple<int, int> lastInput = Tuple.Create(0, 0);
        public override bool EditInput(ref int x, ref int y) {
            int delta = Math.Abs(lastInput.Item1 - x) + Math.Abs(lastInput.Item2 - y);
            lastInput = Tuple.Create(x, y);
            return delta >= strength;
        }

        public override bool Load(dynamic config) {
            if (!config.ContainsKey("antichatter")) {
                return false;
            }
            if (config.antichatter.ContainsKey("enabled")) {
                if (!(bool)config.antichatter.enabled) {
                    return false;
                }
            }
            if (config.antichatter.ContainsKey("strength")) {
                strength = (int)config.antichatter.strength;
            }
            return true;
        }

    }

    public class StatsFilter : Filter
    {

        private long last_ms = 0;
        public override bool EditInput(ref int x, ref int y) {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long difference = milliseconds - last_ms;
            last_ms = milliseconds;
            if (difference == 0) {
                difference = 1;
            }
            Console.Write($"\rX: {x} Y: {y} Report Rate: {(int)1000 / difference} Latency: {difference}ms                                    ");
            return true;
        }

        public override bool Load(dynamic config) {
            if (!config.ContainsKey("stats")) {
                return false;
            }
            if (config.stats.ContainsKey("enabled")) {
                if (!(bool)config.stats.enabled) {
                    return false;
                }
            }
            return true;
        }
    }

    public class NoiseFilter : Filter
    {

        private int strength = 1;
        Random rnd = new Random();

        public override bool EditInput(ref int x, ref int y) {
            x += rnd.Next(-strength, strength);
            y += rnd.Next(-strength, strength);
            return true;
        }

        public override bool Load(dynamic config) {
            if (!config.ContainsKey("noise")) {
                return false;
            }
            if (config.noise.ContainsKey("enabled")) {
                if (!(bool)config.noise.enabled) {
                    return false;
                }
            }
            if (config.noise.ContainsKey("strength")) {
                strength = (int)config.noise.strength;
            }
            return true;
        }

    }

}
