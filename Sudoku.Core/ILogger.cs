namespace Sudoku.Core
{
    public interface ILogger
    {
        string InitializingString { get; }

        void Info(string msg);
        void Error(string msg);
    }
}