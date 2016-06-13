using System;
using System.Linq;

namespace Sudoku.Core
{
    public class Solver
    {

        public Solver(Board board)
        {
            Board = board;
        }

        public Board Board { get; set; }

        public bool SolveEasiestMove()
        {
            var moveSolved = false;
            var max = Enum.GetValues(typeof(Constants.SolveMethod)).Cast<Constants.SolveMethod>().Last();
            for (var method = Constants.SolveMethod.NakedSingle; !moveSolved && method <= max; method++)
            {
                moveSolved = SolveOneMove(method);
            }
            return moveSolved;
        }

        private bool SolveOneMove(Constants.SolveMethod method)
        {
            bool moveSolved;
            switch (method)
            {
                case Constants.SolveMethod.NakedSingle:
                    moveSolved = SolveNakedSingle();
                    break;
                case Constants.SolveMethod.HiddenSingle:
                    moveSolved = SolveHiddenSingle();
                    break;
                default:
                    throw new Exception("Something went wrong. Update Solver.SolveOneMove.switch cases");
            }
            return moveSolved;
        }


        public bool SolvePuzzle()
        {
            var changed = true;
            while (changed)
            {
                changed = SolveEasiestMove();
            }
            return Board.IsSolved();
        }

        /// <summary>
        /// Starting with a random cell, solve the first cell with only one candidate.
        /// </summary>
        /// <returns></returns>
        private bool SolveNakedSingle()
        {
            var changed = false;
            var randomId = Board.RandomCellId();
            for (var i = randomId; !changed && i < Constants.TotalCellCount + randomId - 1; i++)
            {
                var cellId = i % Constants.TotalCellCount + 1;
                var cell = Board.GetCell(cellId);

                if (cell.Value > 0 || !cell.IsSolved())
                {
                    continue;
                }
                Board.SetCellValue(cellId, cell.Candidates.SolvedValue, Constants.SolveMethod.NakedSingle);

                changed = true;
            }
            return changed;
        }

        private bool SolveHiddenSingle()
        {
            //Starting with a random house, look for a candidate that appears in only one cell in a house.

            //TODO: Function misbehaving because candidates are getting ruled out erroneously

            var rand = new Random();

            var randomHouseIndex = rand.Next(27);

            for (var i = randomHouseIndex;
                i < Constants.TotalHouseCount + randomHouseIndex;
                i++)
            {
                var houseIndex = i%Constants.TotalHouseCount;
                var house = Board.Houses[houseIndex];

                var randomVal = rand.Next(Constants.BoardLength) + 1;
                for (var j = randomVal; j < Constants.BoardLength + randomVal; j++)
                {
                    var val = (j-1)%Constants.BoardLength + 1;
                    var candidateCount = 0;
                    Cell lastCell = null;

                    var randomCellIndex = rand.Next(Constants.BoardLength);
                    // TODO: optimize to ignore solved numbers
                    for (var k = randomCellIndex; candidateCount < 2 && k < Constants.BoardLength + randomCellIndex; k++)
                    {
                        var cellIndex = k%Constants.BoardLength;
                        var cell = house.Cells[cellIndex];
                        if (cell.Candidates.Contains(val))
                        {
                            candidateCount++;
                            lastCell = cell;
                        }
                    }
                    if (candidateCount == 1 && lastCell != null && lastCell.Value == 0)
                    {
                        Board.SetCellValue(lastCell.CellId, val, Constants.SolveMethod.HiddenSingle);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
