using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    public static class Logger
    {
        public static void LogVector(Vector3 vec)
        {
            Console.WriteLine(vec.X + ":" + vec.Y + ":" + vec.Z);
        }

        public static void LogWithPrefix(string prefix, string message)
        {
            Console.WriteLine("[" + prefix + "] " + message);
        }

        public static void Engine(string message)
        {
            LogWithPrefix("ENG", message);
        }

        public static void Info(string message)
        {
            LogWithPrefix("INF", message);
        }

        public static void Warning(string message)
        {
            LogWithPrefix("WRN", message);
        }
    }
}
