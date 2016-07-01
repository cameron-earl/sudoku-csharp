using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Core
{
    public abstract class House : IComparable
    {
        public enum HouseType
        {
            Row,
            Column,
            Box
        }

        public abstract HouseType MyHouseType { get; }
 
        public IList<Cell> Cells { get; set; } = new List<Cell>();
        public int HouseNumber { get; set; }
        
        public void Add(Cell cell)
        {
            if (Cells.Contains(cell)) return;
            if (Cells.Count >= Constants.BoardLength)
            {
                throw new Exception($"Something tried to add more than {Constants.BoardLength} cells to a house.");
            }
            Cells.Add(cell);
        }

        public void UpdateCandidates()
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Value == 0) continue;
                for (int j = 0; j < Cells.Count; j++)
                {
                    if (j == i || Cells[j].Value != 0) continue;

                    Cells[j].Candidates.EliminateCandidate(Cells[i].Value);
                }
            }
        }

        public void UpdateCandidates(int val)
        {
            foreach (Cell cell in Cells)
            {

                if (!cell.IsSolved())
                {
                    cell.Candidates.EliminateCandidate(val);
                }
            }
        }

        public abstract override string ToString();

        public int CompareTo(object obj)
        {
            var otherHouse = (House) obj;
            return HouseNumber.CompareTo(otherHouse.HouseNumber);
        }

        public bool IsValid()
        {
            if (Cells.Any(cell => !cell.IsValid)) return false;

            //Check for duplicates
            for (int i = 0; i < Constants.BoardLength - 1; i++)
            {
                for (int j = i + 1; Cells[i].Value > 0 && j < Constants.BoardLength; j++)
                {
                    if (Cells[i].Value == Cells[j].Value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string ToSimpleString()
        {
            string str = $"House: {HouseNumber} - ";
            foreach (var cell in Cells)
            {
                str += $"{cell}";
            }
            return str;
        }

        public bool Contains(int val)
        {
            return Cells.Any(cell => cell.Value == val);
        }

        public int CountCellsWithCandidate(int val)
        {
            int count = 0;
            foreach (Cell cell in Cells)
            {
                if (cell.CouldBe(val)) count++;
            }
            return count;
            //return Contains(val) ? 1 : Cells.Count(cell => cell.CouldBe(val));
        }

        public int CountUnsolvedCells()
        {
            return Cells.Count(cell => cell.Value == 0);
        }
    }
}