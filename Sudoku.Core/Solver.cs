using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sudoku.Core
{
    public class Solver
    {
        private readonly Dictionary<Constants.SolveMethod, int> _moveCount =
            new Dictionary<Constants.SolveMethod, int>();

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
            Constants.SolveMethod max =
                Enum.GetValues(typeof(Constants.SolveMethod)).Cast<Constants.SolveMethod>().Last();
            for (var method = Constants.SolveMethod.NakedSingle; !moveSolved && method <= max; method++)
            {
                moveSolved = SolveOneMove(method);
                if (moveSolved) _moveCount[method]++;
            }
            return moveSolved;
        }

        public string MoveCountsToString()
        {
            return _moveCount.Where(i => i.Value > 0).Aggregate("", (current, i) => current + $"{i.Key} - {i.Value}\n");
        }

        private static Cell[] CellsWithThisCandidateArray(IEnumerable<Cell> inList, int candidate)
        {
            return inList.Where(cell => cell.Candidates.Contains(candidate)).ToArray();
        }
        
        /// <summary>
        /// Uses reflection to call a method by the same name as the provided enum
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private bool SolveOneMove(Constants.SolveMethod method)
        {
            bool moveSolved;
            try
            {
                MethodInfo theMethod = GetType().GetMethod($"{method}", BindingFlags.NonPublic | BindingFlags.Instance);
                moveSolved = (bool) theMethod.Invoke(this, null);
            }
            catch (Exception)
            {
                throw new Exception(
                    $"There was an attempt to call a solving method ({method}) which hasn't been programmed.");
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

        #region Named Solving Methods

        // All these methods must have the exact same name as the corresponding enum in Constants.SolveMethod
        // When coded, make sure to uncomment the matching enum.
        // ReSharper disable UnusedMember.Local

        /// <summary>
        /// Starting with a random cell, solve the first cell with only one candidate.
        /// </summary>
        /// <returns></returns>
        private bool NakedSingle()
        {
            bool changed = false;
            int randCellIndex = Board.RandomCellId() - 1;
            for (int i = randCellIndex; !changed && i < Constants.TotalCellCount + randCellIndex; i++)
            {
                int cellId = i%Constants.TotalCellCount + 1;
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
        private bool HiddenSingle()
        {
            var rand = new Random();

            int randomHouseIndex = rand.Next(27);

            for (int i = randomHouseIndex; i < Constants.TotalHouseCount + randomHouseIndex; i++)
            {
                int houseIndex = i%Constants.TotalHouseCount;
                House house = Board.Houses[houseIndex];

                int randomCandidateIndex = rand.Next(Constants.BoardLength);
                for (int j = randomCandidateIndex; j < Constants.BoardLength + randomCandidateIndex; j++)
                {
                    int val = j%Constants.BoardLength + 1;

                    if (house.Contains(val)) continue;

                    int candidateCount = 0;
                    Cell lastCell = null;

                    int randomCellIndex = rand.Next(Constants.BoardLength);

                    for (int k = randomCellIndex;
                        candidateCount < 2 && k < Constants.BoardLength + randomCellIndex;
                        k++)
                    {
                        int cellIndex = k%Constants.BoardLength;
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
        private bool NakedPair()
        {
            return NakedTuple(2);

            //Not sure if the following is faster or slower. It's certainly simpler, though.
            //var rnd = new Random();
            //House[] houseArray = Board.GetShuffledCopyOfHouseArray(rnd);

            //foreach (House house in houseArray)
            //{
            //    for (int cellIndex = 0; cellIndex < house.Cells.Count - 1; cellIndex++)
            //    {
            //        Cell cell = house.Cells[cellIndex];
            //        //Identify first pair
            //        if (cell.Value != 0 || cell.Candidates.Count() != 2) continue;
            //        for (int cell2Index = cellIndex + 1; cell2Index < house.Cells.Count; cell2Index++)
            //        {
            //            Cell cell2 = house.Cells[cell2Index];
            //            //Identify second pair
            //            if (cell2.Value != 0 || !cell.Candidates.Equals(cell2.Candidates)) continue;
            //            int[] candArr = cell.Candidates.GetCandidateArray();
            //            bool changed = false;
            //            foreach (int val in candArr)
            //            {
            //                foreach (Cell cell3 in house.Cells)
            //                {
            //                    //Eliminate candidates from other cells in the house
            //                    if (!cell.Candidates.Equals(cell3.Candidates))
            //                    {
            //                        changed = cell3.Candidates.EliminateCandidate(val) || changed;
            //                    }
            //                }
            //            }
            //            // Only return success if a candidate has been eliminated
            //            if (changed) return true;
            //        }
            //    }
            //}

            //return false;
        }

        /// <summary>
        /// Starting with a random house, find a pair of candidates that are only contained within the same two cells of that house.
        /// Eliminate all other candidates from those two cells.
        /// </summary>
        /// <returns></returns>
        private bool HiddenPair()
        {
            bool changed = false;

            var rnd = new Random();
            House[] houseArray = Board.GetShuffledCopyOfHouseArray(rnd);

            foreach (House house in houseArray)
            {
                //Get list of candidates ready
                var candidateList = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9};
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
        private bool IntersectionRemoval()
        {
            bool changed = false;

            var rnd = new Random();
            House[] houseArray = Board.GetShuffledCopyOfHouseArray(rnd);

            foreach (House house in houseArray)
            {
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

        /// <summary>
        /// Starting with a random house, find three cells with the same three candidates between them. Remove the candidates from
        /// all other cells in the house.
        /// </summary>
        /// <returns></returns>
        private bool NakedTriple()
        {
            return NakedTuple(3);
        }

        private bool NakedQuad()
        {
            return NakedTuple(4);
        }

        private bool XWing()
        {
            return BasicFish(2);
        }

        private bool SwordFish()
        {
            return BasicFish(3);
        }

        private bool JellyFish()
        {
            return BasicFish(4);
        }

        #endregion


        #region Helper Methods
        private bool NakedTuple(int tuple)
        {
            var rnd = new Random();

            bool changed = false;

            House[] houseArray = Board.GetShuffledCopyOfHouseArray(rnd);

            foreach (House house in houseArray)
            {
                //Get list of house's unsolved candidates ready //TODO: Does this really speed up the code appreciably?
                var candidateList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                foreach (Cell cell in house.Cells)
                {
                    if (cell.Value > 0)
                    {
                        candidateList.Remove(cell.Value);
                    }
                }

                // If the house has <= [tuple] candidates, there aren't any values left to rule out
                if (candidateList.Count <= tuple) continue;

                //Get random-order list of all cells with multiple but at most [tuple] candidates
                List<Cell> cellList = (from cell in house.Cells
                                       let count = cell.Candidates.Count()
                                       where count > 1 && count <= tuple
                                       select cell
                    ).OrderBy(x => rnd.Next()).ToList();

                //If there are less than [tuple] cells in this list, this method is of no use
                if (cellList.Count < tuple) continue;

                // For each combination of three cells in this list, look for a set with only three unique candidates between them

                //create an array of cell indexes {0, 1, 2...}
                int[] indexes = Enumerable.Range(0, tuple).ToArray();


                int pointer = indexes.Length - 1;
                while (indexes[tuple - 1] < cellList.Count)
                {
                    ISet<int> candidateSet = new SortedSet<int>();
                    foreach (int index in indexes)
                    {
                        foreach (int cand in candidateList)
                        {
                            if (cellList[index].Candidates.Contains(cand))
                            {
                                candidateSet.Add(cand);
                            }
                        }
                    }
                    if (candidateSet.Count == tuple) // Success! Remove these candidates from all other cells in house
                    {
                        //trim these three cells from cell list
                        for (int i = indexes.Length - 1; i >= 0; i--)
                        {
                            cellList.RemoveAt(indexes[i]);
                        }
                        //for each cell in the list, remove each candidate in the set
                        foreach (Cell cell in cellList)
                        {
                            foreach (int val in candidateSet)
                            {
                                changed = cell.Candidates.EliminateCandidate(val) || changed;
                            }
                        }
                        if (changed) return true;
                    }

                    // Change the indexes so the next loop will check a different combination of cells
                    /*
                        * increment the last index
                        * while the last index is too high and two neighboring indexes have a difference > 1, 
                        *  find the last cell that is over one lower than the one after it
                        *  increment that cell, and fill all the others after it
                        * if no change is made, the set is ruled out
                        */
                    indexes[tuple - 1]++;
                    while (indexes[tuple - 1] >= cellList.Count && pointer >= 0)
                    {
                        pointer = LastNonConsecutiveIndex(indexes);
                        if (pointer >= 0)
                        {
                            //increment appropriate indexes
                            indexes[pointer]++;
                            for (int i = pointer + 1; i < indexes.Length; i++)
                            {
                                indexes[i] = indexes[pointer] + (i - pointer);
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Starting with a random unsolved candidate, find x lines (base set) 
        /// wherein it appears only within the same x (or less) opposing lines 
        /// (cover set). Remove candidate from all other cells in cover set. 
        /// </summary>
        /// <param name="lineCount"></param>
        /// <returns></returns>
        private bool BasicFish(int lineCount)
        {
            //Start with a random candidate
            int randomValIndex = new Random().Next(9);
            for (int i = randomValIndex; i < 9 + randomValIndex; i++)
            {
                int val = i % 9 + 1;
                if (Board.IsValueSolved(val)) continue;

                //Generate lists of rows and cols in which the candidate appears between 2 and x times
                //There must be at least x lines in both the base and cover sets
                if (val == 5)
                    Console.WriteLine();
                IList<House> rows = (from row in Board.Rows
                                     let count = row.CountCellsWithCandidate(val)
                                     where (count > 1 && count <= lineCount)
                                     select row)
                                     .ToList();
                if (rows.Count < lineCount) continue;


                IList<House> cols = (from col in Board.Columns
                                     let count = col.CountCellsWithCandidate(val)
                                     where (count > 1 && count <= lineCount)
                                     select col)
                                     .ToList();
                if (cols.Count < lineCount) continue;


                if (FindBasicFish(val, lineCount, rows)) return true; // todo make random
                if (FindBasicFish(val, lineCount, cols)) return true;

            }

            return false;
        }

        /// <summary>
        /// Will search given lines and check each combination for a basic fish of given lineCount
        /// </summary>
        /// <param name="val"></param>
        /// <param name="lineCount"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private bool FindBasicFish(int val, int lineCount, IList<House> lines)
        {
            var eliminated = new bool[lines.Count];

            while (true)
            {
                // This loop handles changing earlier lines and exiting with failure
                //While there aren't enough lines left
                while (lines.Count - eliminated.Where(c => c).Count() < lineCount)
                {
                    // make the last 0 value 1, everything after = 0
                    int lastIndex = -1;
                    for (int i = eliminated.Length - 1; lastIndex == -1 && i >= 0; i--)
                    {
                        lastIndex = (!eliminated[i]) ? i : lastIndex;
                    }
                    if (lastIndex == -1)
                        return false; //failure!
                    eliminated[lastIndex] = true;
                    for (int i = lastIndex + 1; i < eliminated.Length; i++)
                    {
                        eliminated[i] = false;
                    }
                }

                //This loop handles changing the last line and exiting with success
                while (!eliminated[eliminated.Length - 1])
                {
                    //get the first x lines that aren't eliminated
                    IList<House> chosenLines = new List<House>();
                    int lastIndex = 0;
                    for (int i = 0; chosenLines.Count < lineCount && i < eliminated.Length; i++)
                    {
                        if (!eliminated[i])
                        {
                            chosenLines.Add(lines[i]);
                        }
                        lastIndex = i;
                    }
                    //send to check function, return true if change made
                    if (CheckForNewFish(val, chosenLines))
                        return true;

                    //eliminate former xth line
                    eliminated[lastIndex] = true;
                }

            }
        }

        /// <summary>
        /// Checks a certain combination of lines for a new fish (of that many lines)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="chosenLines"></param>
        /// <returns></returns>
        private bool CheckForNewFish(int val, IList<House> chosenLines)
        {
            int len = chosenLines.Count;
            bool change = false;
            // gather indexes of each cell with val as candidate for each line
            var coverSet = new SortedSet<int>();
            foreach (House line in chosenLines)
            {
                if (line.Contains(val))
                    return false;
                for (int i = 0; i < line.Cells.Count; i++)
                {
                    if (line.Cells[i].CouldBe(val))
                    {
                        coverSet.Add(i + 1);
                        if (coverSet.Count > len) return false;
                    }
                }
            }
            // if count of indexes < count of lines, throw exception
            if (coverSet.Count < len) throw new Exception("What the what?");

            // otherwise gather cells in coverset (but not base set) into new list
            House.HouseType baseType = chosenLines[0].MyHouseType;
            House.HouseType coverType = (baseType == House.HouseType.Column) ? House.HouseType.Row : House.HouseType.Column;
            List<Cell> baseCells = chosenLines.SelectMany(line => line.Cells).ToList();
            List<Cell> coverCells = (from i in coverSet
                                     select Board.GetHouse(coverType, i - 1)
                                        into line
                                     from cell
                                     in line.Cells
                                     where !baseCells.Contains(cell)
                                     select cell)
                                    .ToList();
            // and eliminate that candidate from each cell in the list
            foreach (Cell cell in coverCells)
            {
                change = cell.Candidates.EliminateCandidate(val) || change;
            }
            // return whether change was made
            return change;
        }
        #endregion


        /// <summary>
        /// Returns the last index that is at least two apart from the one after it
        /// Returns -1 if no such index exists (or the pattern for the whole array is arr[i+1} = arr[i]+1)
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int LastNonConsecutiveIndex(IReadOnlyList<int> arr)
        {
            if (arr.Count < 2) return -2;
            for (int i = arr.Count - 2; i >= 0; i--)
            {
                if (arr[i + 1] - (i + 1) != arr[i] - (i)) return i;
            }
            return -1;
        }

    }
}