using System;
using Sudoku.Core;

namespace FolderTransfer.Logger
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
        }

        /// <summary>
        ///     Constructor only exists to allow calling with null string parameter.
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public ConsoleLogger(string initializingString) : this()
        {
        }

        public void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {msg}");
            Console.ResetColor();
        }

        public string InitializingString => "";

        public void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}