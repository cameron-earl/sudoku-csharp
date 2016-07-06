using System;
using System.Data.SqlClient;
using System.Diagnostics;
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
            //for (int i = 0; i < 10; i++)
            //{
            //    SolvedUpdater(15 -  i);
            //}
            //for (int i = 0; i < 15; i++)
            //{
            //    UnsolvedUpdater(15 -  i);
            //}
            
            SolvedUpdater();

            //TestNewTechnique(Constants.SolvingTechnique.BiValueUniversalGrave);

            //PuzzleImporter();
            //MainMenu();


            WriteLine("Farewell!");
            ReadKey();
        }

        // ReSharper disable once UnusedMember.Local
        private static void TestNewTechnique(Constants.SolvingTechnique technique)
        {
            WriteLine($"Testing {technique}");
            bool falsePositives = Solver.TechniqueHasFalsePositives(technique);
            string str = falsePositives ? "" : "not";
            WriteLine($"{technique} is {str} throwing false positives");
        }

        /// <summary>
        /// Attempts to solve any puzzles in unsolved database, moving them over if successful
        /// Takes a while because all puzzles require lots of technique attempts
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static void UnsolvedUpdater(int numberOfPuzzles = 0)
        {
            Stopwatch runTime = Stopwatch.StartNew();
            string topString = numberOfPuzzles > 0 ? $"TOP {numberOfPuzzles} " : "";
            int newSolveCount = 0;
            int totalUnsolved = 0;
            using (var conn = new SqlConnection(DbHelper.ConnStr))
            {
                var cmd = new SqlCommand($"SELECT {topString}Puzzle FROM dbo.UnsolvedBoards ORDER BY Id DESC", conn);
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
            runTime.Stop();
            TimeSpan ts = runTime.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            WriteLine();
            WriteLine($"{newSolveCount} solved out of {totalUnsolved} in {elapsedTime}");
        }

        /// <summary>
        /// Will review all puzzles in solved database
        /// If puzzle is unsolvable, will move to unsolved database
        /// If hardest move is easier, will update hardest move
        /// Eventually will score puzzles
        /// </summary>
        private static void SolvedUpdater(int numberOfPuzzles = 0)
        {
            Stopwatch runTime = Stopwatch.StartNew();
            
            string topString = numberOfPuzzles > 0 ? $"TOP {numberOfPuzzles} " : "";

            using (var conn = new SqlConnection(DbHelper.ConnStr))
            {
                var cmd = new SqlCommand($"SELECT {topString}Puzzle FROM dbo.Boards ORDER BY Id DESC", conn);
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
            runTime.Stop();
            TimeSpan ts = runTime.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
            WriteLine($"\nSolved database updated in {elapsedTime}.");
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
