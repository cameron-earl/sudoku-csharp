using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Sudoku.Core
{
    public static class DBHelper
    {
        public const string ConnStr =
            "Data Source=(localdb)\\ProjectsV13;Initial Catalog=Sudoku.DB;Integrated Security=True";

        public static bool AddBoardToDatabase(string boardStr)
        {
            boardStr = new Regex("[\\D]").Replace(boardStr, "");
            if (boardStr.Length != 81) throw new Exception($"Sudoku string must be 81 long. This is {boardStr.Length}.");
            var b = new Board(boardStr);
            var solver = new Solver(b);
            bool solved = solver.SolvePuzzle();
            if (!solved) return false;
            string solvedValues = new Regex("[\\D]").Replace(b.ToSimpleString(), "");
            string hardestMove = solver.GetHardestMove();
            using (
                var insertCommand =
                    new SqlCommand(
                        $"INSERT INTO dbo.Boards (Puzzle, SolvedValues, HardestMove) VALUES ('{boardStr}','{solvedValues}','{hardestMove}')", new SqlConnection(ConnStr))
                )
            {
                insertCommand.Connection.Open();
                insertCommand.ExecuteNonQuery();
                insertCommand.Connection.Close();
            }
            return true;
            
        }

    }
}
