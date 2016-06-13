using System;

namespace Sudoku.Core
{
    public class Cell
    {
        private int _value;

        #region Constructors

        public Cell(int cellId, int value)
        {
            if (cellId < 1 || cellId > Constants.TotalCellCount)
            {
                throw new ArgumentOutOfRangeException($"Cell id must be between 1 and {Constants.TotalCellCount}");
            }
            CellId = cellId;
            SetHouses();
            Candidates = new CandidateSet(value);
            Value = value;
            if (value > 0) SolveMethod = Constants.SolveMethod.Provided;
        }

        public int CellId { get; }

        public Cell(int cellId) : this(cellId, 0)
        {
        }

        #endregion

        #region Properties

        public int Value
        {
            get { return _value; }
            internal set
            {
                if (value > Constants.BoardLength)
                {
                    throw new ArgumentOutOfRangeException($"Cell value must be 0-{Constants.BoardLength}.");
                }
                Candidates.SolvedValue = value;
                _value = value;
            }
        }

        public int RowNumber { get; private set; }

        public int ColumnNumber { get; private set; }

        public int BoxNumber { get; private set; }

        public CandidateSet Candidates { get; private set; }

        public Constants.SolveMethod SolveMethod { get; set; } = Constants.SolveMethod.Unsolved;

        public bool IsSolved()
        {
            return (Candidates.SolvedValue > 0);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Initializes houses for cell
        /// </summary>
        /// <returns></returns>
        private void SetHouses() // Only valid with Constants.BoardLength = 9.
        {
            ColumnNumber = GetCellCol(CellId);
            RowNumber = GetCellRow(CellId);

            if (RowNumber <= 3) // BoxNumber 1, 2, or 3
            {
                if (ColumnNumber <= 3) BoxNumber = 1;
                else if (ColumnNumber <= 6) BoxNumber = 2;
                else BoxNumber = 3;
            }
            else if (RowNumber <= 6) // BoxNumber 4, 5, or 6
            {
                if (ColumnNumber <= 3) BoxNumber = 4;
                else if (ColumnNumber <= 6) BoxNumber = 5;
                else BoxNumber = 6;
            }
            else // BoxNumber 7, 8, or 9
            {
                if (ColumnNumber <= 3) BoxNumber = 7;
                else if (ColumnNumber <= 6) BoxNumber = 8;
                else BoxNumber = 9;
            }
        }

        public override string ToString()
        {
            return (Value > 0) ? $"{Value}" : " ";
        }


        #endregion

        #region Static Methods


        public static int GetCellRow(int cellId)
        {
            cellId--;
            return cellId / Constants.BoardLength + 1;
        }

        public static int GetCellCol(int cellId)
        {
            cellId--;
            return cellId % Constants.BoardLength + 1;
        }

        public string CellCoordinateToString()
        {
            return $"{'@'+RowNumber}{ColumnNumber}";
        }

        #endregion

        public static int GetCellId(int row, int col)
        {
            row--;
            col--;
            return row*9 + col + 1;
        }
    }
}
