using System;
using System.Data.SqlClient;
using Sudoku.Core;

namespace Sudoku.ConsoleApp
{
    public class Program
    {
        static void Main()
        {
            //Load sample boards from app.config
            //var sampleBoards = ConfigurationManager.GetSection("sampleBoards") as NameValueCollection;
            //if (sampleBoards == null)
            //{
            //    Console.WriteLine("Please adjust App.config to include a sample board.");
            //    Console.ReadKey();
            //    return;
            //}

            //add sample boards to database
            //foreach (string board in sampleBoards)
            //{
            //    Console.WriteLine($"Adding {board}");

            //    DBHelper.AddBoardToDatabase(sampleBoards[board]);

            //    //SqlCommand insertCommand = new SqlCommand($"INSERT INTO dbo.Boards (Puzzle) VALUES ({board})");
            //}
            //Console.ReadKey();
            //return;

            using (var conn = new SqlConnection(DBHelper.ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Id, Puzzle, TimesPlayed FROM dbo.Boards ORDER BY NEWID()",
                    Connection = conn
                };
                string boardStr = "";
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
                //var randomBoard = new Board(sampleBoards[new Random().Next(sampleBoards.Count)]);
                //var testBoard = new Board(sampleBoards["nakedTriple1"]);

                var testBoard = new Board(boardStr);
                var thisGame = new Game(testBoard);
                Console.WriteLine($"Playing board #{id}");
                Console.ReadKey();
                thisGame.Play();
                playCount++;
                cmd.CommandText = $"UPDATE dbo.Boards SET TimesPlayed = {playCount} WHERE Id = {id}";
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            
            
           
        }
    }

}
