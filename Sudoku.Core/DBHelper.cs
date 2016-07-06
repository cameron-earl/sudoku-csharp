using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Sudoku.Core
{
    public static class DbHelper
    {
        public const string ConnStr =
            "Data Source=(localdb)\\ProjectsV13;Initial Catalog=Sudoku.DB;Integrated Security=True";


        /// <summary>
        /// If a board exists in solved database, will increment playcount.
        /// If a board exists in the unsolved database but is solvable, will move to solved.
        /// If a board is in neither database, will add to the appropriate one.
        /// </summary>
        /// <param name="boardStr"></param>
        /// <returns></returns>
        public static void UpdateBoardInDatabase(string boardStr) //TODO add logger?
        {
            //Validate input
            boardStr = new Regex("[\\D]").Replace(boardStr, "");
            if (boardStr.Length != 81) return;
            var b = new Board(boardStr);
            if (!b.IsValid() || !b.IsUnique() || b.IsSolved())
            {
                //Console.WriteLine("Board not added to database because it's invalid, not unique, or already solved.");
                //Console.ReadKey();
                return;
            }

            //Initialize values, attempt to solve
            var solver = new Solver(b);
            bool isSolved = solver.SolvePuzzle();
            string solvedValues = new Regex("[\\D]").Replace(b.ToSimpleString(), "");
            string thisHardestMove = solver.GetHardestMove();
            int thisHardestMoveCount = solver.GetHardestMoveCount();
            string oldHardestMove = null;
            string newHardestMove = null;
            int hardestMoveCount = -1;
            int unsolvedId = -1;
            int solvedId = -1;
            int timesPlayed = -1;

            //Check database for matching entries
            using (var conn = new SqlConnection(ConnStr))
            {
                //Check solved table for matching entry
                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Id, HardestMove, TimesPlayed, HardestMoveCount FROM dbo.Boards WHERE Puzzle='{boardStr}'",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        solvedId = reader.GetInt32(0);
                        oldHardestMove = reader.GetString(1);
                        timesPlayed = reader.GetInt32(2);
                        hardestMoveCount = reader.GetInt32(3);
                    }
                    conn.Close();
                }

                if (isSolved)
                {
                    newHardestMove = (oldHardestMove == null) ? thisHardestMove : Constants.GetEasiestMove(thisHardestMove, oldHardestMove);
                    
                    // If it was found in database
                    if (solvedId >= 1) 
                    {
                        // Update hardest move
                        if (!newHardestMove.Equals(oldHardestMove))
                        {
                            Console.WriteLine(
                                $"Easier route found: hardest move changed from {oldHardestMove} to {newHardestMove}");
                            hardestMoveCount = thisHardestMoveCount;
                        }
                        else if (thisHardestMoveCount < hardestMoveCount || hardestMoveCount == -1)
                        {
                            hardestMoveCount = thisHardestMoveCount;
                        }
                        

                        //Increment TimesPlayed for existing solved table
                        timesPlayed = (timesPlayed > -1) ? timesPlayed + 1 : timesPlayed;
                        cmd.CommandText = $"UPDATE dbo.Boards SET HardestMove='{newHardestMove}', Score={solver.Score}, TimesPlayed={timesPlayed}, HardestMoveCount={hardestMoveCount} WHERE Id = {solvedId}";
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Console.WriteLine("Board found in database, times played incremented.");
                        //Console.ReadKey();
                        return;
                    } 
                }

                //Check unsolved table for matching entry
                cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Id FROM dbo.UnsolvedBoards WHERE Puzzle='{boardStr}'",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        unsolvedId = reader.GetInt32(0);
                    }
                    conn.Close();
                }

                //Add new board to unsolved table
                if (!isSolved && unsolvedId == -1)
                {
                    cmd = new SqlCommand(
                        $"INSERT INTO dbo.UnsolvedBoards (Puzzle) VALUES ('{boardStr}')",
                        conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    //Console.WriteLine("Board added to unsolved database.");
                    //Console.ReadKey();
                }

                //Add new board to solved table
                else if (isSolved && solvedId == -1)
                {
                    cmd = new SqlCommand(
                        $"INSERT INTO dbo.Boards (Puzzle, Score, SolvedValues, HardestMove, TimesPlayed, HardestMoveCount) VALUES ('{boardStr}',{solver.Score},'{solvedValues}','{newHardestMove}',1,{thisHardestMoveCount})",
                        conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    //Console.WriteLine("Board added to solved database.");
                    //Console.ReadKey();
                }

                //Remove from unsolved table
                if (isSolved && unsolvedId != -1)
                {
                    cmd = new SqlCommand(
                        $"DELETE FROM dbo.UnsolvedBoards WHERE id = {unsolvedId}",
                        conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    //Console.WriteLine("Board removed from unsolved database.");
                    //Console.ReadKey();
                }

                //Remove from solved table
                if (!isSolved && solvedId != -1)
                {
                    cmd = new SqlCommand(
                        $"DELETE FROM dbo.Boards WHERE id = {solvedId}",
                        conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    Console.WriteLine($"Board removed from solveDB: {oldHardestMove}");
                    //Console.ReadKey();
                }

            }

        }

        public static string GetUnsolvedBoard()
        {
            string boardStr = "";
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Puzzle FROM dbo.UnsolvedBoards ORDER BY NEWID()",
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

        public static string GetChallengingBoard()
        {
            string boardStr = "";
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Puzzle FROM dbo.Boards WHERE HardestMove NOT LIKE \'%Single\' ORDER BY NEWID()",
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

        public static string GetEasyBoard()
        {
            string boardStr = "";
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Puzzle FROM dbo.Boards WHERE HardestMove LIKE \'%Single\' ORDER BY NEWID()",
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

        public static string GetRandomBoard()
        {
            string boardStr = "";
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Puzzle FROM dbo.Boards ORDER BY NEWID()",
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

        public static string GetSolvedBoardString(string puzzle)
        {
            puzzle = new Regex("[\\D]").Replace(puzzle, "");
            string solvedBoardString = null;
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 SolvedValues FROM dbo.Boards WHERE Puzzle='{puzzle}'",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        solvedBoardString = reader.GetString(0);
                    }
                    conn.Close();
                }
            }
            return solvedBoardString;
        }

        public static int GetMinSolvedId()
        {
            int minId = -1;
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Id FROM dbo.Boards",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        minId = reader.GetInt32(0);
                    }
                    conn.Close();
                }
            }
            return minId;
        }

        public static int GetMaxSolvedId()
        {
            int maxId = -1;
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = "SELECT TOP 1 Id FROM dbo.Boards ORDER BY Id DESC",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        maxId = reader.GetInt32(0);
                    }
                    conn.Close();
                }
            }
            return maxId;
        }

        public static string GetSolvedBoardById(int inputId)
        {
            
            string puzzle = null;
            using (var conn = new SqlConnection(ConnStr))
            {
                var cmd = new SqlCommand()
                {
                    CommandText = $"SELECT TOP 1 Puzzle FROM dbo.Boards WHERE Id={inputId}",
                    Connection = conn
                };
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        puzzle = reader.GetString(0);
                    }
                    conn.Close();
                }
            }
            return puzzle;
        }
    }
}
