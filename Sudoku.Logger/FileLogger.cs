using System.IO;
using Sudoku.Core;

namespace FolderTransfer.Logger
{
    public class FileLogger : ILogger
    {
        private string _logFile = Path.GetFullPath("log.txt");

        public FileLogger()
        {
        }

        public FileLogger(string initializingString) : this()
        {
            LogFile = initializingString;
        }

        public string InitializingString
        {
            get { return _logFile; }
            set { LogFile = value; }
        }

        public void Error(string msg)
        {
            using (var sw = new StreamWriter(LogFile, true))
                sw.WriteLine($"Error: {msg}");
        }


        public void Info(string msg)
        {
            using (var sw = new StreamWriter(LogFile, true))
                sw.WriteLine(msg);
        }

        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = Path.GetFullPath(value); }
        }

        public void EmptyLog()
        {
            File.WriteAllText(LogFile, string.Empty);
        }
    }
}