using System;
using System.Linq;
using System.Text.RegularExpressions;

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
            foreach (House house in Houses)
            {
                house.UpdateCandidates();
            }
        }

        public Board(string valueString) : this(ConvertStringParameter(new Regex("[\\D]").Replace(valueString, "")))
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

        public string ToSimpleString()
        {
            string str = "";
            foreach (House row in Rows)
            {
                str = row.Cells.Aggregate(str, (current, cell) => current + $"{cell}");
                str += " \n";
            }
            return str;
        }

        public string CandidatesToString()
        {
            string candidateStr = "";
            for (char row = 'A'; row <= 'I'; row++)
            {
                candidateStr += $"{row}: ";
                for (int col = 1; col <= 9; col++)
                {
                    int cellId = Cell.GetCellId((row + 1 -'A'), col);
                    Cell cell = GetCell(cellId);
                    if (cell.Value != 0) continue;
                    CandidateSet candidates = cell.Candidates;
                    candidateStr += $"{col}(";
                    for (int val = 1; val <= 9; val++ )
                    {
                        if (candidates.Contains(val))
                        {
                            candidateStr += $"{val}";
                        }
                    }
                    candidateStr += ") ";
                }
                candidateStr += '\n';
            }
            return candidateStr;
        }
        
        public bool IsSolved()
        {
            return Cells.All(cell => cell.Value != 0) && IsValid();
        }

        public bool IsValid()
        {
            if (IsProvenInvalid) return false;
            foreach (House house in Houses)
            {
                if (!house.IsValid())
                {
                    IsProvenInvalid = true;
                }
                    
            }
            return !IsProvenInvalid;
        }

        public bool IsProvenInvalid { get; private set; }

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

        public void SetCellValue(int cellId, int newValue, Constants.SolveMethod solveMethod)
        {
            Cell cell = GetCell(cellId);
            House row = Rows[cell.RowNumber - 1];
            House col = Columns[cell.ColumnNumber - 1];
            House box = Boxes[cell.BoxNumber - 1];

            //Validate move legality
            if (cell.SolveMethod == Constants.SolveMethod.Provided)
            {
                throw new Exception("Tried to change a provided value");
            }
            if (cell.Value > 0 && (solveMethod != Constants.SolveMethod.PlayerInput
                || (solveMethod == Constants.SolveMethod.PlayerInput && cell.SolveMethod != Constants.SolveMethod.PlayerInput)))
            {
                throw new Exception("Tried to change solved value");
            }
            if (newValue > 0 && !cell.Candidates.Contains(newValue))
            {
                throw new Exception("Tried to change value to an eliminated candidate.");
            }
            if (row.Contains(newValue) || col.Contains(newValue) || box.Contains(newValue))
            {
                throw new Exception("New value is already in house.");
            }

            //Change value and update candidates in its houses
            cell.Value = newValue;
            cell.SolveMethod = solveMethod;
            row.UpdateCandidates(newValue);
            col.UpdateCandidates(newValue);
            box.UpdateCandidates(newValue);
        }

        public House[] GetShuffledCopyOfHouseArray()
        {
            return GetShuffledCopyOfHouseArray(new Random());
        }

        public House[] GetShuffledCopyOfHouseArray(Random rnd)
        {
            var randomArray = new House[Houses.Length];
            Array.Copy(Houses, randomArray, Houses.Length);
            return randomArray.OrderBy(x => rnd.Next()).ToArray();
        }

        public bool IsValueSolved(int val)
        {
            if (val < 1 || val > Constants.BoardLength)
            {
                return false;
            }

            return Rows.All(row => row.Contains(val));
        }

        public House GetHouse(House.HouseType houseType, int index)
        {
            switch (houseType)
            {
                case House.HouseType.Row:
                    return Rows[index];
                case House.HouseType.Column:
                    return Columns[index];
                case House.HouseType.Box:
                    return Boxes[index];
                default:
                    throw new Exception("What the what?");
            }
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

        public static bool IsValidPuzzle(string boardStr)
        {
            boardStr = new Regex("[\\D]").Replace(boardStr, "");
            if (boardStr.Length != 81) return false;
            var testBoard = new Board(boardStr);
            if (testBoard.IsSolved()) return false;
            if (!testBoard.IsValid()) return false;
            if (!testBoard.IsUnique()) return false;
            var solver = new Solver(testBoard);
            return solver.SolvePuzzle() || testBoard.IsUnique();
        }

        public bool IsUnique()
        {
            int containsCount = 0;
            for (int val = 1; val <= Constants.BoardLength; val++)
            {
                if (this.Contains(val)) containsCount++;
            }
            if (containsCount < 8) return false;

            //Other Uniqueness tests here
            //Like unique rectangle test

            return true;
        }

        public bool Contains(int val)
        {
            return Rows.Any(row => row.Contains(val));
        }
    }
    
}
