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
        public static void MakeLog(LogLevel level, string message)
        {
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
            Console.WriteLine(prefix + ": " + message);
        }

        public static void Info(string msg)
        {
            MakeLog(LogLevel.Info, msg);
        }

        public static void Warn(string msg)
        {
            MakeLog(LogLevel.Warn, msg);
        }
        public static void Error(string msg)
        {
            MakeLog(LogLevel.Error, msg);
        }
    }
}
