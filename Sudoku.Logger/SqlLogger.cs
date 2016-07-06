using System.Data.SqlClient;
using Sudoku.Core;

namespace Sudoku.Logger
{
    public class SqlLogger : ILogger
    {
        public SqlLogger(string initializingString)
        {
            ConnStr = initializingString;
        }

        public string InitializingString => ConnStr;

        public void Info(string msg)
        {
            InsertLog(msg, "Info");
        }

        public void Error(string msg)
        {
            InsertLog(msg, "Error");
        }

        public string ConnStr { get; }

        private void InsertLog(string msg, string type)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO SqlLog ([Message], [Type]) VALUES (@msg, @type)";
                    cmd.Parameters.AddWithValue("@msg", msg);
                    cmd.Parameters.AddWithValue("@type", type);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}