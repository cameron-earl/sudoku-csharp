using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sudoku.Core;
using static System.Console;

namespace Sudoku.ConsoleApp
{
    public class Program
    {
        public static void Main()
        {
            //UnsolvedUpdater();

            //WriteLine("Testing TwoStringKite");
            //Solver.TechniqueHasFalsePositives(Constants.SolvingTechnique.TwoStringKite);
            //WriteLine("Any false positives would have thrown an error.");
            //ReadKey();

            //PuzzleImporter();
            MainMenu();
        }

        private static void UnsolvedUpdater()
        {
            int newSolveCount = 0;
            int totalUnsolved = 0;
            using (var conn = new SqlConnection(DBHelper.ConnStr))
            {
                var cmd = new SqlCommand("SELECT Puzzle FROM dbo.UnsolvedBoards", conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        totalUnsolved++;
                        string boardStr = reader.GetString(0);
                        if (new Solver(new Board(boardStr)).SolvePuzzle())
                        {
                            newSolveCount++;
                            DBHelper.AddBoardToDatabase(boardStr);
                            Write(".");
                        }
                    }
                    conn.Close();
                }
            }
            WriteLine();
            WriteLine($"{newSolveCount} solved out of {totalUnsolved}");
            ReadKey();
        }

        private static void PuzzleImporter()
        {
            string[] lines = File.ReadAllLines(@"C:\Users\cameron.earl\Documents\sudokuboards4.txt", Encoding.UTF8);
            int count = 0;
            foreach (string line in lines)
            {
                count++;
                if (count % 500 == 0) WriteLine(count);
                DBHelper.AddBoardToDatabase(line.Replace(".","0"));
            }
        }

        private static void MainMenu()
        {
            
            bool done = false;

            while (!done)
            {
                Clear();
                string board = null;
                Menu.PrintHeading("Welcome to Sudoku!");
                WriteLine("  A. Input a Game");
                WriteLine("  B. Play a Random Game");
                WriteLine("  C. Play an Easy Game");
                WriteLine("  D. Play a Challenging Game");
                WriteLine("  E. Play an Unsolved Game");
                WriteLine("  X. Exit program");

                const string regexStr = "[a-ex]";
                char choice = Menu.GetCharacterInput(new Regex(regexStr));
                switch (choice)
                {
                    case 'a':
                        board = Menu.GetBoardInput();
                        break;
                    case 'b':
                        board = DBHelper.GetRandomBoard();
                        break;
                    case 'c':
                        board = DBHelper.GetEasyBoard();
                        break;
                    case 'd':
                        board = DBHelper.GetChallengingBoard();
                        break;
                    case 'e':
                        board = DBHelper.GetUnsolvedBoard();
                        break;
                    case 'x':
                        done = true;
                        break;
                    default:
                        WriteLine("Something went wrong, try again.");
                        break;
                }
                if (board != null)
                {
                    PlayGame(board);
                }
            }
        }

        private static void PlayGame(string boardStr)
        {
            boardStr = new Regex("[\\D]").Replace(boardStr, "");
            var testBoard = new Board(boardStr);
            var thisGame = new Game(testBoard);
            thisGame.Play();
            DBHelper.AddBoardToDatabase(boardStr);
        }

        

        
    }

}
