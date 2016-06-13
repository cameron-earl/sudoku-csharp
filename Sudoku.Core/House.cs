using System;
using System.Collections.Generic;

namespace Sudoku.Core
{
    public abstract class House
    {
        
 
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
                for (int j = 0; j != i && Cells[i].Value > 0 && j < Cells.Count; j++)
                {
                    Cells[j].Candidates.EliminateCandidate(Cells[i].Value);
                }
            }
        }

        public abstract override string ToString();

        public bool IsValid()
        {
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
    }
}