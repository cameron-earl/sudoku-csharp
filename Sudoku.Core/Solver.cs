using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Sudoku.Core
{
    public class Solver
    {
        #region Fields

        private readonly Dictionary<Constants.SolvingTechnique, int> _moveCount =
            new Dictionary<Constants.SolvingTechnique, int>();
        private static readonly Random Rnd = new Random();
        private Cell[] _shuffledCells;
        private House[] _shuffledHouses;
        private int[] _shuffledValues;
        private Cell[] _shuffledUnsolvedCells;
        private ExceptionDispatchInfo _exceptionDispatchInfo;
        public Constants.SolvingTechnique LastMove { get; private set; }

        #endregion

        #region Technique Properties
        //Everything in this section should be set to null after each successfully used technique.
        
        private Cell[] ShuffledCells
        {
            get
            {
                if (_shuffledCells == null)
                {
                    _shuffledCells = Board.Cells.OrderBy(x => Rnd.Next()).ToArray();
                }
                return _shuffledCells;

            }
            set { _shuffledCells = value; }
        }

        private House[] ShuffledHouses
        {
            get
            {
                if (_shuffledHouses == null)
                {
                    _shuffledHouses = Board.Houses.OrderBy(x => Rnd.Next()).ToArray();
                }
                return _shuffledHouses;
            }
            set { _shuffledHouses = value; }
        }

        private int[] ShuffledValues
        {
            get
            {
                if (_shuffledValues == null)
                {
                    _shuffledValues = Enumerable.Range(1, Constants.BoardLength).OrderBy(x => Rnd.Next()).ToArray();
                }
                return _shuffledValues;
            }
            set { _shuffledValues = value; }
        }

        private Cell[] ShuffledUnsolvedCells
        {
            get
            {
                return _shuffledUnsolvedCells ?? 
                    (_shuffledUnsolvedCells = (from cell in Board.Cells
                                                where !cell.IsSolved()
                                                select cell
                                                ).OrderBy(x => Rnd.Next())
                                                .ToArray());
            }
            set { _shuffledUnsolvedCells = value; }
        }

        #endregion

        #region Constructors
        public Solver(Board board)
        {
            Board = board;
            foreach (Constants.SolvingTechnique method in Enum.GetValues(typeof(Constants.SolvingTechnique)))
            {
                if (method > Constants.SolvingTechnique.Provided) _moveCount.Add(method, 0);
            }
        } 
        #endregion

        #region Properties
        public Board Board { get; set; }
        #endregion

        #region Action/Solving Methods
        public bool SolveEasiestMove()
        {
            bool moveSolved = false;
            for (var method = Constants.SolvingTechnique.NakedSingle;
                    !moveSolved && method < Constants.SolvingTechnique.Unsolved;
                    method++)
            {
                moveSolved = SolveOneMove(method);
            }
            return moveSolved;
        }
        
        /// <summary>
        /// Uses reflection to call a method by the same name as the provided enum
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private bool SolveOneMove(Constants.SolvingTechnique method)
        {
            bool moveSolved;
            try
            {
                MethodInfo theMethod = GetType().GetMethod($"{method}", BindingFlags.NonPublic | BindingFlags.Instance);
                moveSolved = (bool)theMethod.Invoke(this, null);
            }
            catch (NullReferenceException)
            {
                throw new Exception(
                    $"There was an attempt to call a solving method ({method}) which hasn't yet been programmed.");
            }
            _exceptionDispatchInfo?.Throw();
            if (moveSolved)
            {
                _moveCount[method]++;
                ResetTechniqueFields();
                LastMove = method;
            }
            return moveSolved;
        }

        private void ResetTechniqueFields()
        {
            ShuffledCells = null;
            ShuffledUnsolvedCells = null;
            ShuffledHouses = null;
            ShuffledValues = null;
        }
        
        public bool SolvePuzzle()
        {
            bool changed = true;
            if (Board.IsInvalidatedBySolvedBoard() || Board.IsProvenInvalid) return false;
            while (changed)
            {
                changed = SolveEasiestMove();
            }
            return Board.IsSolved();
        }

        private bool EliminateCandidate(Cell cell, int val)
        {
            bool changed;
            try
            {
                changed = cell.Candidates.EliminateCandidate(val);
            }
            catch (SolvingException ex)
            {
                changed = false;
                _exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            return changed;
        }

        private bool SetCellValue(Cell cell, int val, Constants.SolvingTechnique technique)
        {
            bool changed;
            try
            {
                Board.SetCellValue(cell, val, technique);
                changed = true;
            }
            catch (SolvingException ex)
            {
                changed = false;
                _exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            return changed;
        }

        #endregion
        
        #region Diagnostic Methods

        //The following methods are useful in analyzing the puzzle's current state

        public string MoveCountsToString()
        {
            return _moveCount.Where(i => i.Value > 0).Aggregate("", (current, i) => current + $"{i.Key} - {i.Value}\n");
        }

        public string GetHardestMove()
        {
            try
            {
                return _moveCount.Last(i => i.Value > 0).Key.ToString();
            }
            catch (Exception)
            {
                return "No good move known.";
            }
        }

        #endregion

        #region Named Solving Techniques

        // All these methods must have the exact same name as the corresponding enum in Constants.SolvingTechnique
        // When coded, make sure to uncomment the matching enum.
        // ReSharper disable UnusedMember.Local

        /// <summary>
        /// Starting with a random cell, solve the first cell with only one candidate.
        /// </summary>
        /// <returns></returns>
        private bool NakedSingle()
        {
            
            bool changed = false;
            foreach (Cell cell in ShuffledUnsolvedCells)
            {
                if (cell.Candidates.SolvedValue == 0) continue;

                SetCellValue(cell, cell.Candidates.SolvedValue, Constants.SolvingTechnique.NakedSingle);

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
            foreach (House house in ShuffledHouses)
            {

                foreach (int val in ShuffledValues)
                {
                    if (house.Contains(val)) continue;

                    int candidateCount = 0;
                    Cell lastCell = null;
                    
                    foreach (Cell cell in house.Cells)
                    {
                        if (cell.CouldBe(val))
                        {
                            candidateCount++;
                            lastCell = cell;
                        }
                    }

                    if (candidateCount == 1  && lastCell != null)
                    {
                        SetCellValue(lastCell, val, Constants.SolvingTechnique.HiddenSingle);
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
        }

        /// <summary>
        /// Starting with a random house, find a pair of candidates that are only contained within the same two cells of that house.
        /// Eliminate all other candidates from those two cells.
        /// </summary>
        /// <returns></returns>
        private bool HiddenPair()
        {
            return HiddenTuple(2);
        }

        /// <summary>
        /// Starting with a random house, find a candidate that is only found in cells contained in another single house.
        /// Remove that candidate from all cells in the other house.
        /// </summary>
        /// <returns></returns>
        private bool IntersectionRemoval()
        {
            bool changed = false;
            
            House[] houseArray = Board.GetShuffledCopyOfHouseArray(Rnd);

            foreach (House house in houseArray)
            {
                //Get list of house's unsolved candidates ready
                var candidateList = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9};
                foreach (Cell cell in house.Cells)
                {
                    if (cell.IsSolved())
                    {
                        candidateList.Remove(cell.Value);
                    }
                }

                //for each candidate, get a list of cells with that candidate
                foreach (int val in candidateList)
                {
                    Cell[] cellArr = CellsWithThisCandidateArray(house.Cells, val);
                    if (cellArr.Length > 3 || cellArr.Length == 0) continue;

                    //Check if each cell in list shares another house
                    int boxNum = cellArr[0].BoxNumber;
                    int rowNum = cellArr[0].RowNumber;
                    int colNum = cellArr[0].ColumnNumber;
                    bool shareBox = true;
                    bool shareRow = true;
                    bool shareCol = true;
                    for (int cellIndex = 1; cellIndex < cellArr.Length; cellIndex++)
                    {
                        shareBox = shareBox && boxNum == cellArr[cellIndex].BoxNumber;
                        shareRow = shareRow && rowNum == cellArr[cellIndex].RowNumber;
                        shareCol = shareCol && colNum == cellArr[cellIndex].ColumnNumber;
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
                            if (!cellArr.Contains(cell))
                            {
                                changed = EliminateCandidate(cell, val) || changed;
                            }
                        }
                    }
                    if (shareRow)
                    {
                        foreach (Cell cell in Board.Rows[rowNum - 1].Cells)
                        {
                            if (!cellArr.Contains(cell))
                            {
                                changed = EliminateCandidate(cell, val) || changed;
                            }
                        }
                    }
                    if (shareCol)
                    {
                        foreach (Cell cell in Board.Columns[colNum - 1].Cells)
                        {
                            if (!cellArr.Contains(cell))
                            {
                                changed = EliminateCandidate(cell, val) || changed;
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

        private bool HiddenTriple()
        {
            return HiddenTuple(3);
        }

        private bool HiddenQuad()
        {
            return HiddenTuple(4);
        }

        private bool Skyscraper()
        {
            //Start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;

                IList<IList<House>> rowsAndCols = new List<IList<House>>(2);
                //Generate lists of rows and cols in which the candidate appears exactly twice
                //There must be at least two in one set for it to work
                IList<House> rows = (from row in Board.Rows
                                     let count = row.CountCellsWithCandidate(val)
                                     where (count == 2)
                                     select row)
                                     .OrderBy(a => Rnd.Next())
                                     .ToList();
                if (rows.Count >= 2) rowsAndCols.Add(rows);


                IList<House> cols = (from col in Board.Columns
                                     let count = col.CountCellsWithCandidate(val)
                                     where (count == 2)
                                     select col)
                                     .OrderBy(a => Rnd.Next())
                                     .ToList();
                if (cols.Count >= 2) rowsAndCols.Add(cols);

                // Starting randomly with rows or cols, find a skyscraper
                IList<House>[] shuffledRowsAndCols = rowsAndCols.OrderBy(a => Rnd.Next()).ToArray();
                foreach (IList<House> lineSet in shuffledRowsAndCols)
                {
                    int[] indexes = Enumerable.Range(0, 2).ToArray();
                    while (indexes[indexes.Length - 1] > 0)
                    {
                        IList<House> chosenLines = indexes.Select(index => lineSet[index]).ToList();
                        if (CheckForSkyscraper(val, chosenLines))
                            return true;
                        indexes = GetNextCombination(indexes, lineSet.Count);
                    }
                }

            }

            return false;
        }

        private bool TwoStringKite()
        {
            //Start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;
                
                //Generate lists of rows and cols in which the candidate appears exactly twice
                //There must be at least 1 line in both the rows and columns
                IList<House> rows = (from row in Board.Rows
                                     let count = row.CountCellsWithCandidate(val)
                                     where (count == 2)
                                     select row)
                                     .ToList();
                if (!rows.Any()) continue;


                IList<House> cols = (from col in Board.Columns
                                     let count = col.CountCellsWithCandidate(val)
                                     where (count == 2)
                                     select col)
                                     .ToList();
                if (!cols.Any()) continue;

                if ((from row in rows from col in cols where CheckForTwoStringKite(val, row, col) select row).Any())
                {
                    return true;
                }

            }

            return false;
        }

        private bool YWing()
        {
            return Wing(false);
        }

        // ReSharper disable once InconsistentNaming
        private bool XYZWing()
        {
            return Wing(true);
        }

        private bool SimpleColoring()
        {
            //start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;

                //build a complete set of cells in houses that hold candidate exactly twice
                IList<Cell> chainSeeds = new List<Cell>();
                foreach (House house in Board.Houses)
                {
                    if (house.CountCellsWithCandidate(val) == 2)
                    {
                        foreach (Cell cell in house.Cells)
                        {
                            if (cell.CouldBe(val) && !chainSeeds.Contains(cell))
                            {
                                chainSeeds.Add(cell);
                            }
                        }
                    }
                }
                if (!chainSeeds.Any()) continue;

                IList<Cell> shuffledChainSeeds = chainSeeds.OrderBy(x => Rnd.Next()).ToList();
                while (shuffledChainSeeds.Count >= 1)
                {

                    //Use those cells as seeds to build blue and green chains
                    List<Cell>[] coloringChains = BuildColoringChains(shuffledChainSeeds[0], val);

                    //Check all cells in each chain to see if any cells can see eachother
                    if (coloringChains[0].Count > 1 && coloringChains[1].Count > 1)
                    {
                        for (int j = 0; j < coloringChains.Length; j++)
                        {
                            int otherIndex = (j + 1) % 2;
                            int[] indexes = Enumerable.Range(0, 2).ToArray();
                            while (indexes[1] != 0)
                            {
                                if (coloringChains[j][indexes[0]].CanSee(coloringChains[j][indexes[1]]))
                                {
                                    //solve all cells of the other color
                                    foreach (Cell cell in coloringChains[otherIndex])
                                    {
                                        SetCellValue(cell, val, Constants.SolvingTechnique.SimpleColoring);
                                    }
                                    return true;
                                }

                                indexes = GetNextCombination(indexes, coloringChains[j].Count);
                            }
                        }
                    }

                    //if both are internally consistent, check all other unsolved cells not in a chain if they see cells of both colors. 
                    //if so, remove candidate from cell
                    bool changed = false;
                    foreach (Cell cell in Board.Cells)
                    {
                        if (cell.IsSolved()
                            || !cell.CouldBe(val)
                            || coloringChains[0].Contains(cell)
                            || coloringChains[1].Contains(cell))
                        {
                            continue;
                        }
                        //check if the cell can see a cell in both chains
                        bool sawOne = false;
                        for (int index = 0; !sawOne && index < coloringChains[0].Count; index++)
                        {
                            Cell chainCell = coloringChains[0][index];
                            sawOne = cell.CanSee(chainCell);
                        }
                        if (sawOne == false) continue;
                        sawOne = false;
                        for (int index = 0; !sawOne && index < coloringChains[1].Count; index++)
                        {
                            Cell chainCell = coloringChains[1][index];
                            sawOne = cell.CanSee(chainCell);
                        }
                        if (sawOne == false) continue;

                        //if (Board.SolvedBoard.GetCell(cell.CellId).Value == val) throw new Exception("I was about to do something bad.");
                        changed = EliminateCandidate(cell, val) || changed;
                    }
                    if (changed) return true;

                    //if not, remove all cells in blue and green chains from shuffledChainSeeds and start with a new seed
                    foreach (List<Cell> coloringChain in coloringChains)
                    {
                        foreach (Cell cell in coloringChain)
                        {
                            shuffledChainSeeds.Remove(cell);
                        }
                    } 
                }
            }
            return false;
        }

        // ReSharper disable once InconsistentNaming
        private bool WXYZWing()
        {
            //Start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;

                //Get shuffled list of all cells which contain val 3 and 1 or 2 other candidates
                IList<Cell> cellsToCheck = ShuffledUnsolvedCells
                    .Where(cell => cell.CouldBe(val) && !cell.IsSolved() && cell.Candidates.Count() <= 3)
                    .ToList();
                if (cellsToCheck.Count < 3) continue;

                //Check each combination of 3 of these cells
                int[] indexes = Enumerable.Range(0, 3).ToArray();
                while (indexes[2] != 0)
                {
                    Cell[] pincerCells = { cellsToCheck[indexes[0]], cellsToCheck[indexes[1]], cellsToCheck[indexes[2]] };
                    if (CheckForWXYZWing(val, pincerCells)) return true;
                    indexes = GetNextCombination(indexes, cellsToCheck.Count);
                }
            }
            return false;
        }
        
        //private bool BowmanBingo() //TODO BROKEN
        //{
        //    if (Board.IsSolved()) return false;
        //    //Pick a random candidate in a random cell to test
        //    Cell cell = ShuffledUnsolvedCells[0];
        //    int candidate = cell.Candidates.GetCandidateArray().OrderBy(x => Rnd.Next()).FirstOrDefault();
        //    //Create new board and attempt to solve
        //    var testBoard = new Board(Board);
        //    testBoard.SetCellValue(cell.CellId, candidate, Constants.SolvingTechnique.BowmanBingo);
        //    var testSolver = new Solver(testBoard);
        //    try
        //    {
        //        testSolver.SolvePuzzle();
        //    }
        //    catch (SolvingException)
        //    {
        //        testBoard.IsProvenInvalid = true;
        //        _exceptionDispatchInfo = null;
        //    }
        //    //If it finds a contradiction, rule out that candidate and return true
        //    if (!testBoard.IsValid())
        //    {
        //        EliminateCandidate(cell, candidate);
        //        return true;
        //    }
        //    //If it solves correctly, set cell to candidate value and return true
        //    if (testBoard.IsSolved())
        //    {
        //        SetCellValue(cell, candidate, Constants.SolvingTechnique.BowmanBingo);
        //        return true;
        //    }
        //    return false; //Failure!
        //}

        #endregion

        #region Technique Helper Methods

        //The following methods are generalizations of a type of a group of methods above.

        private bool NakedTuple(int tuple)
        {
            bool changed = false;

            foreach (House house in ShuffledHouses)
            {
                //If there are less than tuple*2 cells, there should be a corresponding HiddenTuple of smaller size.
                //But that doesn't seem to be the case.

                //Get list of house's unsolved candidates ready
                var candidateList = new List<int>();
                for (int val = 1; val <= Constants.BoardLength; val++)
                {
                    if (!house.Contains(val))
                    {
                        candidateList.Add(val);
                    }
                }

                // If the house has <= tuple candidates, there is nothing to eliminate
                if (candidateList.Count <= tuple) continue;

                //Get random-order list of all cells with multiple but at most [tuple] candidates
                List<Cell> cellList = (from cell in house.Cells
                                       let count = cell.Candidates.Count()
                                       where count > 1 && count <= tuple
                                       select cell
                                        ).OrderBy(x => Rnd.Next()).ToList();

                //If there are less than [tuple] cells in this list, this method is of no use
                if (cellList.Count < tuple) continue;

                // For each combination of [tuple] cells in this list, look for a set with only [tuple] unique candidates between them

                //create an array of cell indexes {0, 1, 2...}
                int[] indexes = Enumerable.Range(0, tuple).ToArray();
                
                while (indexes[indexes.Length - 1] > 0)
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
                                changed = EliminateCandidate(cell, val) || changed;
                            }
                        }
                        if (changed) return true;
                    }

                    indexes = GetNextCombination(indexes, cellList.Count);
                }
            }

            return false;
        }

        /// <summary>
        /// x candidates can only be found in x cells of the same house, eliminating all other candidates in those x cells
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private bool HiddenTuple(int tuple)
        {
            foreach (House house in ShuffledHouses)
            {
                //If there are less than tuple*2 cells, there should be a corresponding NakedTuple of smaller size.
                //But that doesn't seem to be the case.

                //Get list of candidates ready
                var candidateList = new List<int>();
                for (int val = 1; val <= Constants.BoardLength; val++)
                {
                    if (!house.Contains(val) && house.CountCellsWithCandidate(val) <= tuple)
                    {
                        candidateList.Add(val);
                    }
                }

                //If there are less than <= tuple candidates, there is nothing to eliminate
                if (candidateList.Count <= tuple) continue;

                //check each combination of the remaining candidates to see if they are in the same cells
                int[] indexes = Enumerable.Range(0, tuple).ToArray();
                while (indexes[indexes.Length - 1] > 0)
                {
                    //Needs to be a set
                    ISet<Cell> cellSet = new SortedSet<Cell>();
                    foreach (Cell cell in house.Cells)
                    {
                        foreach (int index in indexes)
                        {
                            if (cell.CouldBe(candidateList[index]))
                            {
                                cellSet.Add(cell);
                            }
                        }
                    }
                    if (cellSet.Count == tuple) //Success! Remove all other candidates from those cells
                    {
                        bool changed = false;
                        IList<int> candidates = indexes.Select(index => candidateList[index]).ToList();

                        foreach (Cell cell in cellSet)
                        {
                            for (int val = 1; val <= Constants.BoardLength; val++)
                            {
                                if (candidates.Contains(val)) continue;
                                changed = EliminateCandidate(cell, val) || changed;
                            }
                        }
                        if (changed)
                            return true;
                    }

                    indexes = GetNextCombination(indexes, candidateList.Count);
                }

            }

            return false;
        }

        /// <summary>
        /// Starting with a random unsolved candidate, find x lines (base set) 
        /// wherein it appears only within the same x (or less) opposing lines 
        /// (cover set). Remove candidate from all other cells in cover set. 
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private bool BasicFish(int tuple)
        {
            //Start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;

                //Generate lists of rows and cols in which the candidate appears between 2 and tuple times
                //There must be at least x lines in both the base and cover sets
                IList<House> rows = (from row in Board.Rows
                                     let count = row.CountCellsWithCandidate(val)
                                     where (count > 1 && count <= tuple)
                                     select row)
                                     .OrderBy(a => Rnd.Next())
                                     .ToList();
                if (rows.Count < tuple) continue;


                IList<House> cols = (from col in Board.Columns
                                     let count = col.CountCellsWithCandidate(val)
                                     where (count > 1 && count <= tuple)
                                     select col)
                                     .OrderBy(a => Rnd.Next())
                                     .ToList();
                if (cols.Count < tuple) continue;

                // Starting randomly with rows or cols, find a basic fish
                IList<House>[] rowsAndCols = {rows, cols};
                IList<House>[] shuffledRowsAndCols = rowsAndCols.OrderBy(a => Rnd.Next()).ToArray();
                foreach (IList<House> lineSet in shuffledRowsAndCols)
                {
                    int[] indexes = Enumerable.Range(0, tuple).ToArray();
                    while (indexes[indexes.Length - 1] > 0)
                    {
                        IList<House> chosenLines = indexes.Select(index => lineSet[index]).ToList();
                        if (CheckForNewFish(val, chosenLines))
                            return true;
                        indexes = GetNextCombination(indexes, lineSet.Count);
                    }
                }

            }

            return false;
        }

        // ReSharper disable once InconsistentNaming
        private bool Wing(bool isXYZWing)
        {
            //Start with a random candidate
            foreach (int val in ShuffledValues)
            {
                if (Board.IsValueSolved(val)) continue;

                //Get shuffled list of all cells which contain val and one other candidate
                IList<Cell> cellsToCheck = ShuffledUnsolvedCells
                    .Where(cell => cell.CouldBe(val) && cell.Candidates.Count() == 2)
                    .ToList();
                if (cellsToCheck.Count < 2) continue;

                //Check each combination of 2 of these cells
                int[] indexes = Enumerable.Range(0, 2).ToArray();
                while (indexes[1] != 0)
                {
                    Cell[] pincerCells = { cellsToCheck[indexes[0]], cellsToCheck[indexes[1]] };
                    if (!isXYZWing && CheckForYWing(val, pincerCells)) return true;
                    if (isXYZWing && CheckForXYZWing(val, pincerCells)) return true;
                    indexes = GetNextCombination(indexes, cellsToCheck.Count);
                }
            }
            return false;
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
                change = EliminateCandidate(cell, val) || change;
            }
            // return whether change was made
            return change;
        }

        private bool CheckForSkyscraper(int val, IEnumerable<House> chosenLines)
        {
            bool change = false;
            
            // create set of chosen 4 cells
            IList<Cell> chosenCells = (from line in chosenLines from cell in line.Cells where cell.CouldBe(val) select cell).ToList();
            IList<Cell> endCells = new List<Cell>(2);

            //make sure there's a link between the two lines
            int maxCount = 1;
            foreach (Cell chosenCell in chosenCells)
            {
                int count = chosenCells.Count(c => chosenCell.CanSee(c));
                if (count == 1) endCells.Add(chosenCell);
                maxCount = Math.Max(count, maxCount);
            }
            if (maxCount != 2) return false;

            // gather cells which see two of these cells into a list
            IList<Cell> otherCells = new List<Cell>();
            foreach (Cell otherCell in Board.Cells)
            {
                if (otherCell.IsSolved()) continue;
                if (!otherCell.CouldBe(val)) continue;
                if (chosenCells.Contains(otherCell)) continue;
                int seeCount = endCells.Count(endCell => otherCell.CanSee(endCell));
                if (seeCount == 2) otherCells.Add(otherCell);
            }

            // and eliminate that candidate from each cell in the second list
            foreach (Cell cell in otherCells)
            {
                change = EliminateCandidate(cell, val) || change;
            }
            // return whether change was made
            return change;
        }
        
        private bool CheckForTwoStringKite(int val, House row, House col)
        {
            //Get cells of note in row and column
            IList<Cell> rowCells = row.Cells.Where(cell => cell.CouldBe(val)).ToList();
            IList<Cell> colCells = col.Cells.Where(cell => cell.CouldBe(val)).ToList();

            //Make sure that none of the cells are the same
            //Make sure that (only) one combination of cells from row and col share a box and are not the same point
            int boxCount = 0;
            int box = -1;
            
            foreach (Cell rowCell in rowCells)
            {
                foreach (Cell colCell in colCells)
                {
                    if (rowCell.Equals(colCell)) return false;
                    if (rowCell.BoxNumber == colCell.BoxNumber)
                    {
                        boxCount++;
                        box = rowCell.BoxNumber;
                    }

                }
            }
            if (boxCount != 1) return false;
            //Identify end points & cell that can see both
            int colNum = -1;
            int rowNum = -1;
            foreach (Cell rowCell in rowCells)
            {
                if (rowCell.BoxNumber != box) colNum = rowCell.ColumnNumber;
            }
            foreach (Cell colCell in colCells)
            {
                if (colCell.BoxNumber != box) rowNum = colCell.RowNumber;
            }
            Cell newCell = Board.GetCell(Cell.GetCellId(rowNum, colNum));

            //if it has this candidate, remove it
            bool changed = false;
            if (!newCell.IsSolved() && newCell.CouldBe(val))
            {
                changed = EliminateCandidate(newCell, val);
            }

            //return whether a change was made
            return changed;
        }

        private bool CheckForYWing(int val, IReadOnlyList<Cell> pincerCells)
        {
            if (pincerCells[0].Candidates.Equals(pincerCells[1].Candidates)) return false;
            
            //identify a & b
            int[] hingeValues = (from pincerCell in pincerCells
                                      from candidate in pincerCell.Candidates.GetCandidateArray()
                                      where candidate != val
                                      select candidate)
                                      .ToArray();

            //find all cells which see both, could be the shared value, aren't solved
            //while doing so, look for hinge
            IList<Cell> seeingCells = new List<Cell>();
            bool foundHinge = false;
            foreach (Cell cell in Board.Cells)
            {
                if (cell.CanSee(pincerCells[0]) && cell.CanSee(pincerCells[1]) && !cell.IsSolved())
                {
                    if (cell.CouldBe(hingeValues[0]) && cell.CouldBe(hingeValues[1]) && cell.Candidates.Count() == 2)
                    {
                        foundHinge = true;
                    }
                    else if (cell.CouldBe(val))
                    {
                        seeingCells.Add(cell);
                    }
                }
            }
            if (!foundHinge || !seeingCells.Any()) return false;

            // Remove candidate from all cells in list
            foreach (Cell seeingCell in seeingCells)
            {
                EliminateCandidate(seeingCell, val);
            }
            return true;
        }

        // ReSharper disable once InconsistentNaming
        private bool CheckForXYZWing(int val, IReadOnlyList<Cell> pincerCells)
        {
            if (pincerCells[0].Candidates.Equals(pincerCells[1].Candidates)) return false;

            //identify a & b
            int[] hingeValues = (from pincerCell in pincerCells
                                 from candidate in pincerCell.Candidates.GetCandidateArray()
                                 where candidate != val
                                 select candidate)
                                      .ToArray();
            
            //find hinge
            Cell hinge = Board.Cells.FirstOrDefault(cell => cell
                            .CanSee(pincerCells[0]) 
                            && cell.CanSee(pincerCells[1]) 
                            && cell.CouldBe(val) 
                            && cell.CouldBe(hingeValues[0]) 
                            && cell.CouldBe(hingeValues[1]) 
                            && cell.Candidates.Count() == 3);
            if (hinge == null) return false;

            //find all cells which see all hinge and pincers, have value, and aren't solved
            IList<Cell> seeingCells = Board.Cells
                                        .Where(cell => cell.CanSee(pincerCells[0]) 
                                            && cell.CanSee(pincerCells[1]) 
                                            && cell.CanSee(hinge) 
                                            && !cell.IsSolved() 
                                            && cell.CouldBe(val))
                                        .ToList();
            if (!seeingCells.Any()) return false;

            // Remove candidate from all cells in list
            foreach (Cell seeingCell in seeingCells)
            {
                EliminateCandidate(seeingCell, val);
            }
            return true;
        }

        public List<Cell>[] BuildColoringChains(Cell seed, int val)
        {
            List<Cell>[] coloringChains = {new List<Cell>(), new List<Cell>() };
            coloringChains[0].Add(seed);
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < coloringChains[0].Count; i++)
                {
                    Cell cell = coloringChains[0][i];
                    IList<House> cellHouses = new List<House>();
                    cellHouses.Add(Board.GetHouse(House.HouseType.Row, cell.RowNumber -1));
                    cellHouses.Add(Board.GetHouse(House.HouseType.Column, cell.ColumnNumber - 1));
                    cellHouses.Add(Board.GetHouse(House.HouseType.Box, cell.BoxNumber - 1));
                    foreach (House house in cellHouses)
                    {
                        if (house.CountCellsWithCandidate(val) == 2)
                        {
                            foreach (Cell c in house.Cells)
                            {
                                if (c.CouldBe(val) && !c.Equals(cell) && !coloringChains[1].Contains(c))
                                {
                                    coloringChains[1].Add(c);
                                    changed = true;
                                }
                            }
                        }
                    }
                }
                
                for (int i = 0; i < coloringChains[1].Count; i++)
                {
                    Cell cell = coloringChains[1][i];
                    IList<House> cellHouses = new List<House>();
                    cellHouses.Add(Board.GetHouse(House.HouseType.Row, cell.RowNumber - 1));
                    cellHouses.Add(Board.GetHouse(House.HouseType.Column, cell.ColumnNumber - 1));
                    cellHouses.Add(Board.GetHouse(House.HouseType.Box, cell.BoxNumber - 1));
                    foreach (House house in cellHouses)
                    {
                        if (house.CountCellsWithCandidate(val) == 2)
                        {
                            foreach (Cell c in house.Cells)
                            {
                                if (c.CouldBe(val) && !c.Equals(cell) && !coloringChains[0].Contains(c))
                                {
                                    coloringChains[0].Add(c);
                                    changed = true;
                                }
                            }
                        }
                    }
                }

            } while (changed);
            return coloringChains;
        }

        // ReSharper disable once InconsistentNaming
        private bool CheckForWXYZWing(int val, IReadOnlyList<Cell> pincerCells)
        {
            //verify no pincer cells have identical candidates //TODO necessary?
            if (pincerCells[0].Candidates.Equals(pincerCells[1].Candidates)
                || pincerCells[0].Candidates.Equals(pincerCells[2].Candidates)
                || pincerCells[1].Candidates.Equals(pincerCells[2].Candidates))
            {
                return false;
            }

            //identify W, X, and Y
            IList<int> hingeValues = new List<int>();
            foreach (Cell pincerCell in pincerCells)
            {
                foreach (int candidate in pincerCell.Candidates.GetCandidateArray())
                {
                    if (candidate != val && !hingeValues.Contains(candidate))
                    {
                        hingeValues.Add(candidate);
                    }
                }
            }
            if (hingeValues.Count != 3) return false;

            //check for other non-restricted digits // todo
            foreach (int hingeValue in hingeValues)
            {
                int[] indexes = Enumerable.Range(0, 2).ToArray();
                while (indexes[indexes.Length - 1] > 0)
                {
                    Cell cell1 = pincerCells[indexes[0]];
                    Cell cell2 = pincerCells[indexes[1]];
                    if (cell1.CouldBe(hingeValue)
                        && cell2.CouldBe(hingeValue)
                        && !cell1.CanSee(cell2))
                    {
                        return false;
                    }
                    indexes = GetNextCombination(indexes, pincerCells.Count);
                }
            }

            //find hinge
            Cell hinge = Board.Cells
                .FirstOrDefault(cell => cell.CanSee(pincerCells[0])
                                        && cell.CanSee(pincerCells[1])
                                        && cell.CanSee(pincerCells[2])
                                        && cell.CouldBe(hingeValues[0])
                                        && cell.CouldBe(hingeValues[1])
                                        && cell.CouldBe(hingeValues[2])
                                        && ((cell.Candidates.Count() == 4 && cell.CouldBe(val)) 
                                            || cell.Candidates.Count() == 3));
            if (hinge == null) return false;

            //find all cells which see all hinge and pincers, have value, and aren't solved
            IList<Cell> seeingCells;
            if (hinge.CouldBe(val))
            {
                seeingCells = Board.Cells
                    .Where(cell => cell.CanSee(pincerCells[0])
                                   && cell.CanSee(pincerCells[1])
                                   && cell.CanSee(pincerCells[2])
                                   && cell.CanSee(hinge)
                                   && !cell.IsSolved()
                                   && cell.CouldBe(val))
                    .ToList();
            }
            //or just the pincers
            else
            {
                seeingCells = Board.Cells
                    .Where(cell => cell.CanSee(pincerCells[0])
                                   && cell.CanSee(pincerCells[1])
                                   && cell.CanSee(pincerCells[2])
                                   && !cell.IsSolved()
                                   && cell.CouldBe(val))
                    .ToList();
            }
            if (!seeingCells.Any()) return false;

            // Remove candidate from all cells in list
            foreach (Cell seeingCell in seeingCells)
            {
                if (Board.SolvedBoard?.Cells?[0] != null)
                {
                    if (Board.SolvedBoard.GetCell(seeingCell.CellId).Value == val)
                    {
                        throw new SolvingException("blurg!");
                    }
                }
                EliminateCandidate(seeingCell, val);
            }
            return true;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Takes an array with a combination of indexes [0,1,2,3] and properly increments it. [0,1,2,4]
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="indexCount">Typically "myList.Count"</param>
        /// <returns>The incremented array, or all zeroes if that was the last combination</returns>
        public static int[] GetNextCombination(int[] indexes, int indexCount)
        {

            //increment the last index
            indexes[indexes.Length - 1]++;

            //while the last index is too high and two neighboring indexes have a difference > 1
            //  find the last cell that is over one lower than the one after it
            //  increment that cell, and fill all the others after it
            //  if no change is made, the set is ruled out


            int pointer = LastNonConsecutiveIndex(indexes); ;
            while (indexes[indexes.Length - 1] >= indexCount && pointer >= 0)
            {
                if (pointer >= 0)
                {
                    //increment appropriate indexes
                    indexes[pointer]++;
                    for (int i = pointer + 1; i < indexes.Length; i++)
                    {
                        indexes[i] = indexes[pointer] + (i - pointer);
                    }
                }
                pointer = LastNonConsecutiveIndex(indexes);
            }
            if (indexes[indexes.Length - 1] >= indexCount)
            {
                indexes = new int[indexes.Length];
            }
            return indexes;
        }


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

        private static Cell[] CellsWithThisCandidateArray(IEnumerable<Cell> inList, int candidate)
        {
            return inList.Where(cell => cell.Candidates.Contains(candidate)).ToArray();
        }

        public static bool TechniqueHasFalsePositives(Constants.SolvingTechnique tech)
        {
            IList<Constants.SolvingTechnique> techs = new List<Constants.SolvingTechnique>();
            techs.Add(Constants.SolvingTechnique.NakedSingle);
            techs.Add(Constants.SolvingTechnique.HiddenSingle);
            techs.Add(tech);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
                var game = new Board(DbHelper.GetChallengingBoard());
                var solver = new Solver(game);
                bool changed = true;
                while (changed)
                {
                    foreach (Constants.SolvingTechnique technique in techs)
                    {
                        changed = solver.SolveOneMove(technique);
                        if (!game.IsValid() || game.IsInvalidatedBySolvedBoard())
                        {
                            return true;
                        }
                        if (changed) break;
                    }
                }
                //if (tech == Constants.SolvingTechnique.BowmanBingo && !game.IsSolved()) return false;
                if (!solver.GetHardestMove().Equals(tech.ToString()))
                {
                    i--;
                }
            }


            return false;
        }

        #endregion
        
    }
}