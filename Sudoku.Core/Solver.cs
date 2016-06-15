using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Core
{
    public class Solver
    {
        private readonly Dictionary<Constants.SolveMethod, int> _moveCount = new Dictionary<Constants.SolveMethod,int>();

        public Solver(Board board)
        {
            Board = board;
            foreach (Constants.SolveMethod method in Enum.GetValues(typeof(Constants.SolveMethod)))
            {
                if (method > Constants.SolveMethod.Provided) _moveCount.Add(method, 0);
            }
        }

        public Board Board { get; set; }

        public bool SolveEasiestMove()
        {
            bool moveSolved = false;
            Constants.SolveMethod max = Enum.GetValues(typeof(Constants.SolveMethod)).Cast<Constants.SolveMethod>().Last();
            for (var method = Constants.SolveMethod.NakedSingle; !moveSolved && method <= max; method++)
            {
                moveSolved = SolveOneMove(method);
                if (moveSolved) _moveCount[method]++;
            }
            return moveSolved;
        }

        public string MoveCountsToString()
        {
            return _moveCount.Aggregate("", (current, move) => current + $"{move.Key} - {move.Value}\n");
        }

        private static Cell[] CellsWithThisCandidateArray(IEnumerable<Cell> inList, int candidate)
        {
            return inList.Where(cell => cell.Candidates.Contains(candidate)).ToArray();
        }

        private bool SolveOneMove(Constants.SolveMethod method)
        {
            bool moveSolved;
            switch (method)
            {
                case Constants.SolveMethod.NakedSingle:
                    moveSolved = FindNakedSingle();
                    break;
                case Constants.SolveMethod.HiddenSingle:
                    moveSolved = FindHiddenSingle();
                    break;
                case Constants.SolveMethod.NakedPair:
                    moveSolved = FindNakedPair();
                    break;
                case Constants.SolveMethod.HiddenPair:
                    moveSolved = FindHiddenPair();
                    break;
                case Constants.SolveMethod.IntersectionRemoval:
                    moveSolved = FindIntersectionRemoval();
                    break;
                default:
                    throw new Exception("Something went wrong. Update Solver.SolveOneMove.switch cases");
            }
            return moveSolved;
        }

        public bool SolvePuzzle()
        {
            bool changed = true;
            while (changed)
            {
                changed = SolveEasiestMove();
            }
            return Board.IsSolved();
        }

        #region Solving Methods
        /// <summary>
        /// Starting with a random cell, solve the first cell with only one candidate.
        /// </summary>
        /// <returns></returns>
        private bool FindNakedSingle()
        {
            bool changed = false;
            int randCellIndex = Board.RandomCellId() - 1;
            for (int i = randCellIndex; !changed && i < Constants.TotalCellCount + randCellIndex; i++)
            {
                int cellId = i % Constants.TotalCellCount + 1;
                Cell cell = Board.GetCell(cellId);

                if (cell.Value > 0 || !cell.IsSolved())
                {
                    continue;
                }
                Board.SetCellValue(cellId, cell.Candidates.SolvedValue, Constants.SolveMethod.NakedSingle);

                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Starting with a random cell in a random house, look for a candidate that appears in only one cell in a house and solve.
        /// </summary>
        /// <returns></returns>
        private bool FindHiddenSingle()
        {
            var rand = new Random();

            int randomHouseIndex = rand.Next(27);

            for (int i = randomHouseIndex; i < Constants.TotalHouseCount + randomHouseIndex; i++)
            {
                int houseIndex = i % Constants.TotalHouseCount;
                House house = Board.Houses[houseIndex];

                int randomCandidateIndex = rand.Next(Constants.BoardLength);
                for (int j = randomCandidateIndex; j < Constants.BoardLength + randomCandidateIndex; j++)
                {
                    int val = j % Constants.BoardLength + 1;

                    if (house.Contains(val)) continue;

                    int candidateCount = 0;
                    Cell lastCell = null;

                    int randomCellIndex = rand.Next(Constants.BoardLength);

                    for (int k = randomCellIndex; candidateCount < 2 && k < Constants.BoardLength + randomCellIndex; k++)
                    {
                        int cellIndex = k % Constants.BoardLength;
                        Cell cell = house.Cells[cellIndex];
                        if (cell.Candidates.Contains(val))
                        {
                            candidateCount++;
                            lastCell = cell;
                        }
                    }

                    if (candidateCount == 1
                        && lastCell != null
                        && lastCell.Value == 0
                        && (lastCell.Candidates.SolvedValue == val || lastCell.Candidates.SolvedValue == 0))
                    {
                        Board.SetCellValue(lastCell.CellId, val, Constants.SolveMethod.HiddenSingle);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Starting with a random house, find a pair of cells with the same two candidates. 
        /// If found, eliminate candidates from remainder of house.
        /// </summary>
        /// <returns></returns>
        private bool FindNakedPair()
        {

            var rand = new Random();
            int randomHouseIndex = rand.Next(27);

            for (int i = randomHouseIndex; i < Constants.TotalHouseCount + randomHouseIndex; i++)
            {
                int houseIndex = i % Constants.TotalHouseCount;
                House house = Board.Houses[houseIndex];

                for (int cellIndex = 0; cellIndex < house.Cells.Count - 1; cellIndex++)
                {
                    Cell cell = house.Cells[cellIndex];
                    //Identify first pair
                    if (cell.Value != 0 || cell.Candidates.Count() != 2) continue;
                    for (int cell2Index = cellIndex + 1; cell2Index < house.Cells.Count; cell2Index++)
                    {
                        Cell cell2 = house.Cells[cell2Index];
                        //Identify second pair
                        if (cell2.Value != 0 || !cell.Candidates.Equals(cell2.Candidates)) continue;
                        int[] candArr = cell.Candidates.GetCandidateArray();
                        bool changed = false;
                        foreach (int val in candArr)
                        {
                            foreach (Cell cell3 in house.Cells)
                            {
                                //Eliminate candidates from other cells in the house
                                if (!cell.Candidates.Equals(cell3.Candidates))
                                {
                                    changed = cell3.Candidates.EliminateCandidate(val) || changed;
                                }
                            }
                        }
                        // Only return success if a candidate has been eliminated
                        if (changed) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Starting with a random house, find a pair of candidates that are only contained within the same two cells of that house.
        /// Eliminate all other candidates from those two cells.
        /// </summary>
        /// <returns></returns>
        private bool FindHiddenPair()
        {
            //TODO seems to work but isn't fully tested
            bool changed = false;

            var rand = new Random();
            int randomHouseIndex = rand.Next(27);

            for (int i = randomHouseIndex; i < Constants.TotalHouseCount + randomHouseIndex; i++)
            {
                int houseIndex = i % Constants.TotalHouseCount;
                House house = Board.Houses[houseIndex];

                //Get list of candidates ready
                var candidateList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                foreach (Cell cell in house.Cells)
                {
                    if (cell.Value > 0)
                    {
                        candidateList.Remove(cell.Value);
                    }
                }
                if (candidateList.Count < 3) continue;

                //check each combination of those remaining candidates
                for (int valueIndex1 = 0; valueIndex1 < candidateList.Count - 1; valueIndex1++)
                {
                    int val1 = candidateList[valueIndex1];
                    Cell[] cellsWithFirstCandidate = CellsWithThisCandidateArray(house.Cells, val1);
                    if (cellsWithFirstCandidate.Length != 2) continue;
                    //We now have a list of the only two cells in the house with the first candidate

                    for (int valueIndex2 = valueIndex1 + 1; valueIndex2 < candidateList.Count; valueIndex2++)
                    {
                        int val2 = candidateList[valueIndex2];
                        Cell[] cellsWithSecondCandidate = CellsWithThisCandidateArray(house.Cells, val2);
                        if (cellsWithSecondCandidate.Length != 2) continue;

                        if (!cellsWithFirstCandidate.Contains(cellsWithSecondCandidate[0])
                            || !cellsWithFirstCandidate.Contains(cellsWithSecondCandidate[1]))
                        {
                            continue;
                        }
                        //At this point, we found a match: Two values that are only found as candidates for these two cells in the house

                        foreach (Cell cell in cellsWithSecondCandidate)
                        {
                            for (int val = 1; val <= Constants.BoardLength; val++)
                            {
                                if (val != val1 && val != val2)
                                {
                                    changed = cell.Candidates.EliminateCandidate(val) || changed;
                                }
                            }
                        }

                        if (changed) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Starting with a random house, find a candidate that is only found in cells contained in another single house.
        /// Remove that candidate from all cells in the other house.
        /// </summary>
        /// <returns></returns>
        private bool FindIntersectionRemoval()
        {
            bool changed = false;

            var rand = new Random();
            int randomHouseIndex = rand.Next(27);

            for (int i = randomHouseIndex; i < Constants.TotalHouseCount + randomHouseIndex; i++)
            {
                int houseIndex = i%Constants.TotalHouseCount;
                House house = Board.Houses[houseIndex];

                //Get list of house's unsolved candidates ready
                var candidateList = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9};
                foreach (Cell cell in house.Cells)
                {
                    if (cell.Value > 0)
                    {
                        candidateList.Remove(cell.Value);
                    }
                }

                //for each candidate, get a list of cells with that candidate
                foreach (int val in candidateList)
                {
                    Cell[] cellList = CellsWithThisCandidateArray(house.Cells, val);
                    if (cellList.Length > 3) continue;

                    //Check if each cell in list shares another house
                    int boxNum = cellList[0].BoxNumber;
                    int rowNum = cellList[0].RowNumber;
                    int colNum = cellList[0].ColumnNumber;
                    bool shareBox = true;
                    bool shareRow = true;
                    bool shareCol = true;
                    for (int cellIndex = 1; cellIndex < cellList.Length; cellIndex++)
                    {
                        shareBox = shareBox && boxNum == cellList[cellIndex].BoxNumber;
                        shareRow = shareRow && rowNum == cellList[cellIndex].RowNumber;
                        shareCol = shareCol && colNum == cellList[cellIndex].ColumnNumber;
                    }
                    // If less than two are true, exit
                    if (shareBox ? !(shareRow || shareCol) : !(shareRow && shareCol))
                    {
                        continue;
                    }
                    // Otherwise, remove candidate from all other cells in two shared houses
                    if (shareBox)
                    {
                        foreach (Cell cell in Board.Boxes[boxNum - 1].Cells)
                        {
                            if (!cellList.Contains(cell))
                            {
                                changed = cell.Candidates.EliminateCandidate(val) || changed;
                            }
                        }
                    }
                    if (shareRow)
                    {
                        foreach (Cell cell in Board.Rows[rowNum - 1].Cells)
                        {
                            if (!cellList.Contains(cell))
                            {
                                changed = cell.Candidates.EliminateCandidate(val) || changed;
                            }
                        }
                    }
                    if (shareCol)
                    {
                        foreach (Cell cell in Board.Columns[colNum - 1].Cells)
                        {
                            if (!cellList.Contains(cell))
                            {
                                changed = cell.Candidates.EliminateCandidate(val) || changed;
                            }
                        }
                    }

                    if (changed) return true;

                }

            }
            return false;
        }

        #endregion




    }
}
