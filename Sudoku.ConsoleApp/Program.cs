using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Sudoku.Core;
using static System.Console;

namespace Sudoku.ConsoleApp
{
    public class Program
    {
        public static void Main()
        {
            

            MainMenu();

            
            
            
           
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
                WriteLine("  D. Play a Medium Game");
                WriteLine("  E. Play an Unsolved Game");
                WriteLine("  X. Exit program");

                const string regexStr = "[a-ex]";
                char choice = Menu.GetCharacterInput(new Regex(regexStr));
                switch (choice)
                {
                    case 'a':
                        board = GetBoardInput();
                        break;
                    case 'b':
                        board = GetRandomBoard();
                        break;
                    case 'c':
                        board = GetEasyBoard();
                        break;
                    case 'd':
                        board = GetMediumBoard();
                        break;
                    case 'e':
                        board = GetHardBoard();
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

            using (var conn = new SqlConnection(DBHelper.ConnStr))
            {

                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Id, Puzzle, TimesPlayed FROM dbo.Boards WHERE Puzzle='{boardStr}'",
                    Connection = conn
                };
                int id = -1;
                int playCount = 0;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                        boardStr = reader.GetString(1);
                        playCount = reader.GetInt32(2);
                    }
                    conn.Close();
                }

                var testBoard = new Board(boardStr);
                var thisGame = new Game(testBoard);
                thisGame.Play();
                playCount++;
                if (id >= 1)
                {
                    cmd.CommandText = $"UPDATE dbo.Boards SET TimesPlayed = {playCount} WHERE Id = {id}";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                else if (testBoard.IsSolved())// add board
                {
                    Solver solver = thisGame.Solver;
                    boardStr = new Regex("[\\D]").Replace(boardStr, "");
                    string hardestMove = solver.GetHardestMove();
                    string solvedValues = new Regex("[\\D]").Replace(testBoard.ToSimpleString(), "");

                    cmd = new SqlCommand()
                    {
                        CommandText = $"INSERT INTO dbo.Boards (Puzzle, SolvedValues, HardestMove, TimesPlayed) VALUES ('{boardStr}','{solvedValues}','{hardestMove}',1)",
                        Connection = conn
                    };
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    WriteLine($"Board is solvable and added to database. Hardest move is {hardestMove}");
                    ReadKey();
                }
                
            }
        }

        private static string GetHardBoard()
        {
            //Load sample boards from app.config
            var sampleBoards = ConfigurationManager.GetSection("sampleBoards") as NameValueCollection;
            if (sampleBoards == null)
            {
                Console.WriteLine("Please adjust App.config to include a sample board.");
                Console.ReadKey();
                return null;
            }
            return sampleBoards.Get(new Random().Next(sampleBoards.Count));

        }

        private static string GetMediumBoard()
        {
            throw new NotImplementedException();
        }

        private static string GetEasyBoard()
        {
            throw new NotImplementedException();
        }

        private static string GetRandomBoard()
        {
            string boardStr = "";
            using (var conn = new SqlConnection(DBHelper.ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Puzzle FROM dbo.Boards ORDER BY NEWID()",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        boardStr = reader.GetString(0);
                    }
                    conn.Close();
                }
            }
            return boardStr;
        }

        private static string GetBoardInput()
        {
            bool isValid = false;
            string boardStr = "";
            while (!isValid)
            {
                WriteLine("Please enter a Sudoku puzzle. There should be 81 numbers, with blanks represented as zeroes.");
                string rawInput = ReadLine() + "";
                boardStr = new Regex("[\\D]").Replace(rawInput, "");
                if (boardStr.Length == 81) isValid = true;
            }

            if (!Board.IsValidPuzzle(boardStr))
            {
                boardStr = null;
                WriteLine("The board you entered is invalid. Try again.");
            }

            if (boardStr == null) return null;

            // Add board to database
            var board = new Board(boardStr);
            var solver = new Solver(board);
            if (!solver.SolvePuzzle()) return boardStr; //TODO add to unsolvable database
            string hardestMove = solver.GetHardestMove();
            string solvedValues = new Regex("[\\D]").Replace(board.ToSimpleString(), "");

            using (var conn = new SqlConnection(DBHelper.ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = $"INSERT INTO dbo.Boards (Puzzle, SolvedValues, HardestMove) VALUES ('{boardStr}','{solvedValues}','{hardestMove}')",
                    Connection = conn
                };
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                WriteLine($"Board is now added to database! Hardest move is {hardestMove}");
                ReadKey();
            }

            return boardStr;

        }
    }

}
