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
            SolvedUpdater();

            UnsolvedUpdater();

            //TestNewTechnique(Constants.SolvingTechnique.SueDeCoq);

            //PuzzleImporter();
            //MainMenu();
        }

        // ReSharper disable once UnusedMember.Local
        private static void TestNewTechnique(Constants.SolvingTechnique technique)
        {
            WriteLine($"Testing {technique}");
            bool falsePositives = Solver.TechniqueHasFalsePositives(technique);
            string str = falsePositives ? "" : "not";
            WriteLine($"{technique} is {str} throwing false positives");
            ReadKey();
        }

        /// <summary>
        /// Attempts to solve any puzzles in unsolved database, moving them over if successful
        /// Takes a while because all puzzles require lots of technique attempts
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static void UnsolvedUpdater()
        {
            int newSolveCount = 0;
            int totalUnsolved = 0;
            using (var conn = new SqlConnection(DbHelper.ConnStr))
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
                            DbHelper.UpdateBoardInDatabase(boardStr);
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

        /// <summary>
        /// Will review all puzzles in solved database
        /// If puzzle is unsolvable, will move to unsolved database
        /// If hardest move is easier, will update hardest move
        /// Eventually will score puzzles
        /// </summary>
        private static void SolvedUpdater()
        {
            using (var conn = new SqlConnection(DbHelper.ConnStr))
            {
                var cmd = new SqlCommand("SELECT Puzzle FROM dbo.Boards", conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string boardStr = reader.GetString(0);
                        DbHelper.UpdateBoardInDatabase(boardStr);
                    }
                    conn.Close();
                }
            }
            WriteLine("\nSolved database updated.");
            ReadKey();
        }

        /// <summary>
        /// Used to mine text file where each line is a sudoku board
        /// Boards are added to appropriate database (solved, unsolved, or none)
        /// Format can use '0' or '.' for unknown values
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static void PuzzleImporter()
        {
            string[] lines = File.ReadAllLines(@"C:\Users\cameron.earl\Documents\sudokuboards4.txt", Encoding.UTF8);
            int count = 0;
            foreach (string line in lines)
            {
                count++;
                if (count % 500 == 0) WriteLine(count);
                DbHelper.UpdateBoardInDatabase(line.Replace(".","0"));
            }
        }

        // ReSharper disable once UnusedMember.Local
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
                WriteLine("  F. Play a specific board in database");
                WriteLine("  X. Exit program");

                const string regexStr = "[a-fx]";
                char choice = Menu.GetCharacterInput(new Regex(regexStr));
                switch (choice)
                {
                    case 'a':
                        board = Menu.GetBoardInput();
                        break;
                    case 'b':
                        board = DbHelper.GetRandomBoard();
                        break;
                    case 'c':
                        board = DbHelper.GetEasyBoard();
                        break;
                    case 'd':
                        board = DbHelper.GetChallengingBoard();
                        break;
                    case 'e':
                        board = DbHelper.GetUnsolvedBoard();
                        break;
                    case 'f':
                        board = GetSolvedBoardById();
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

        private static string GetSolvedBoardById()
        {
            int min = DbHelper.GetMinSolvedId();
            int max = DbHelper.GetMaxSolvedId();
            bool isValid = false;
            string boardStr = null;
            while (!isValid)
            {
                int input = Menu.GetIntInput(min, max);
                boardStr = DbHelper.GetSolvedBoardById(input);
                if (boardStr != null) isValid = true;
                if (!isValid) WriteLine("Board with specified id does not exist. Try again.");
            }
            return boardStr;
        }

        private static void PlayGame(string boardStr)
        {
            boardStr = new Regex("[\\D]").Replace(boardStr, "");
            var testBoard = new Board(boardStr);
            var thisGame = new Game(testBoard);
            thisGame.Play();
            DbHelper.UpdateBoardInDatabase(boardStr);
        }

        

        
    }

}
