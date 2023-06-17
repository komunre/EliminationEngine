using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace EliminationEngine
{
    public enum LogLevel
    {
        Info,
        Warn,
        Error,
    }
    public static class Logger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void MakeLog(LogLevel level, string message)
        {
            var time = DateTime.Now;
            var timeString = "[" + time.Hour + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond + "]";
            string prefix = "[INFO]";
            switch (level)
            {
                case LogLevel.Info:
                    prefix = "[INFO]";
                    break;
                case LogLevel.Warn:
                    prefix = "[WARN]";
                    break;
                case LogLevel.Error:
                    prefix = "[ERROR]";
                    break;
            }
            Console.WriteLine(timeString + " " + prefix + ": " + message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Info(string msg)
        {
            MakeLog(LogLevel.Info, msg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Warn(string msg)
        {
            MakeLog(LogLevel.Warn, msg);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Error(string msg)
        {
            MakeLog(LogLevel.Error, msg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LogVector(Vector3 vec, string msg = "")
        {
            MakeLog(LogLevel.Info, msg + " --- " + vec.X + ":" + vec.Y + ":" + vec.Z);
        }
    }
}
