using System;
using System.Collections.Specialized;
using System.Configuration;
using Sudoku.Core;

namespace Sudoku.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sampleBoards = ConfigurationManager.GetSection("sampleBoards") as NameValueCollection;
            if (sampleBoards == null)
            {
                Console.WriteLine("Please adjust App.config to include a sample board.");
               
            };
            var testBoard = new Board(sampleBoards["hard4"]);
            var thisGame = new Game(testBoard);
            thisGame.Play();
        }
    }

}
