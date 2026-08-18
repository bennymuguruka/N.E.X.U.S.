using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public delegate void LogHandler(string logEntry);
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            string text = $"[{DateTime.Now:HH:mm:ss}] {message}";

            Console.WriteLine(text);

            File.AppendAllText("nexus_log.txt", text + Environment.NewLine);
        }
    }

    public static class MonitorLog
    {
        private static readonly object logLock = new object();
        private static readonly List<string> entries = new List<string>();
        private const int MaxEntries = 10;

        public static void Add(string message)
        {
            lock (logLock)
            {
                entries.Add(message);
                if (entries.Count > MaxEntries)
                {
                    entries.RemoveAt(0);
                }
            }
        }

        public static List<string> GetRecent()
        {
            lock (logLock)
            {
                return new List<string>(entries);
            }
        }
    }

    public class ConsoleLogger : ILogger
    {
        public event LogHandler Logged;

        public void Log(string message)
        {
            string text = $"[{DateTime.Now:HH:mm:ss}] {message}";

            File.AppendAllText("nexus_log.txt", text + Environment.NewLine);
            Logged?.Invoke(text);
        }
    }
}
