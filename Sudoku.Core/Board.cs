using System;
using System.Linq;

namespace Sudoku.Core
{
    public class Board
    {
        public Board(int[] values)
        {
            //Validate inputs
            if (values.Length != Constants.TotalCellCount)
            {
                throw new ArgumentException("Provided array must have 81 values.");
            }
            if (values.Max() > Constants.BoardLength)
            {
                throw new ArgumentOutOfRangeException($"Values must be between 0 and {Constants.BoardLength}");
            }

            //initialize board
            for (int i = 0; i < Constants.BoardLength; i++)
            {
                Rows[i] = new Row(i+1);
                Columns[i] = new Column(i+1);
                Boxes[i] = new Box(i+1);
                Houses[i*3 + 0] = Rows[i];
                Houses[i*3 + 1] = Columns[i];
                Houses[i*3 + 2] = Boxes[i];
            }

            for (int i = 0; i < Constants.TotalCellCount; i++)
            {
                Cells[i] = new Cell(i+1, values[i]);

                Rows[Cells[i].RowNumber - 1].Add(Cells[i]);
                Columns[Cells[i].ColumnNumber - 1].Add(Cells[i]);
                Boxes[Cells[i].BoxNumber - 1].Add(Cells[i]);
            }

            //remove impossible candidates in each house
            foreach (var house in Houses)
            {
                house.UpdateCandidates();
            }
        }

        public Board(string valueString) : this(ConvertStringParameter(valueString.Replace(" ","")))
        {
        }

        public Cell[] Cells { get; set; } = new Cell[Constants.TotalCellCount];

        public House[] Rows  { get; set; } = new House[Constants.BoardLength];
        public House[] Columns { get; set; } = new House[Constants.BoardLength];
        public House[] Boxes { get; set; } = new House[Constants.BoardLength];

        public House[] Houses { get; set; } = new House[Constants.BoardLength * 3];

        

        #region Methods
        
        public override string ToString()
        {
            var boardString = "    1   2   3   4   5   6   7   8   9\n";
               boardString += "  ╔═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╗\n";

            for (var row = 0; row < Constants.BoardLength; row++)
            {
                if (row == 3 || row == 6)
                {
                    boardString += "  ╠═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╣\n";
                }
                boardString += Rows[row];
                if (row%3 != 2)
                {
                    boardString += "  ╠───┼───┼───╬───┼───┼───╬───┼───┼───╣\n"; 
                }
            }
            boardString += "  ╚═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╝\n";
            return boardString;
        }

        public bool IsSolved()
        {
            return Cells.All(cell => cell.Value != 0) && IsValid();
        }

        public bool IsValid()
        {
            foreach (var house in Houses)
            {
                if (!house.IsValid()) return false;
            }
            return true;
        }

        public Cell GetCell(string coord)
        {
            int row = coord[0] - '@';
            int col = int.Parse(coord[1].ToString());
            int cellId = Cell.GetCellId(row, col);
            return GetCell(cellId);
        }

        public Cell GetCell(int cellId)
        {
            return (Cells[cellId - 1]);
        }

        #endregion 

        #region Static Methods

        private static int[] ConvertStringParameter(string valueString)
        {
            var charValues = valueString.ToCharArray();
            var intValues = new int[Constants.TotalCellCount];
            for (var i = 0; i < Constants.TotalCellCount; i++)
            {
                intValues[i] = (int)char.GetNumericValue(charValues[i]);
            }
            return intValues;
        }

        public static int RandomCellId()
        {
            return new Random().Next(81) + 1;
        }

        #endregion

        public void SetCellValue(int cellId, int newValue, Constants.SolveMethod solveMethod)
        {
            var cell = GetCell(cellId);
            if (cell.Value > 0 && (solveMethod != Constants.SolveMethod.PlayerInput 
                || (solveMethod == Constants.SolveMethod.PlayerInput && cell.SolveMethod != Constants.SolveMethod.PlayerInput)))
            {
                throw new Exception("Tried to change solved value");
            }
            cell.Value = newValue;
            cell.SolveMethod = solveMethod;
            Rows[cell.RowNumber - 1].UpdateCandidates();
            Columns[cell.ColumnNumber - 1].UpdateCandidates();
            Boxes[cell.BoxNumber - 1].UpdateCandidates();
        }


    }
    
}
