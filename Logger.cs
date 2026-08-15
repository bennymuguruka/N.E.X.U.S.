using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PRG_281_Project.Interfaces;

namespace PRG_281_Project
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            string text = $"[{DateTime.Now:HH:mm:ss}] {message}";

            Console.WriteLine(text);

            File.AppendAllText("nexus_log.txt", text + Environment.NewLine);
        }
    }
}
