using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests
{
    [TestFixture]
    public class SolverTests
    {
        #region LastNonConsecutiveIndex Tests

        [Test]
        public void ShouldReturnNegativeOne()
        {
            //arrange
            int[] arr = {1, 2, 3, 4, 5};
            int expected = -1;
            //act
            int actual = Solver.LastNonConsecutiveIndex(arr);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnZero()
        {
            //arrange
            int[] arr = { 0, 2, 3, 4, 5 };
            int expected = 0;
            //act
            int actual = Solver.LastNonConsecutiveIndex(arr);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnThree()
        {
            //arrange
            int[] arr = { 0, 1, 2, 3, 5 };
            int expected = 3;
            //act
            int actual = Solver.LastNonConsecutiveIndex(arr);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion

        #region GetNextCombination Tests

        [Test]
        public void ShouldIncrementLastIndexOne()
        {
            //arrange
            int listSize = 4;
            int[] indexes = Enumerable.Range(0, 3).ToArray();
            int[] expected = {0, 1, 3};
            //act
            indexes = Solver.GetNextCombination(indexes, listSize);
            //assert
            Assert.That(indexes, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldIncrementMiddleIndex()
        {
            //arrange
            int listSize = 4;
            int[] indexes = { 0, 1, 3 };
            int[] expected = { 0, 2, 3 };
            //act
            indexes = Solver.GetNextCombination(indexes, listSize);
            //assert
            Assert.That(indexes, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnZeroes()
        {
            //arrange
            int listSize = 4;
            int[] indexes = { 1, 2, 3 };
            int[] expected = { 0, 0, 0 };
            //act
            indexes = Solver.GetNextCombination(indexes, listSize);
            //assert
            Assert.That(indexes, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnZeroesForBadValues()
        {
            //arrange
            int listSize = 4;
            int[] indexes = { 2, 3, 4 };
            int[] expected = { 0, 0, 0 };
            //act
            indexes = Solver.GetNextCombination(indexes, listSize);
            //assert
            Assert.That(indexes, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnZeroes2()
        {
            //arrange
            int listSize = 2;
            int[] indexes = { 0, 1 };
            int[] expected = { 0, 0 };
            //act
            indexes = Solver.GetNextCombination(indexes, listSize);
            //assert
            Assert.That(indexes, Is.EqualTo(expected));
        }

        #endregion

        #region BuildColoringChains Tests

        [Test]
        public static void ShouldBuildColoringChainsCorrectly1()
        {
            //arrange - recreate first example puzzle on http://hodoku.sourceforge.net/en/tech_col.php
            const string boardStr =
                "214006000 007902004 000407000 001870032 002690000 048021006 420709861 009168000 186240009";
            var board = new Board(boardStr);
            board.GetCell("C7").Candidates.EliminateCandidate(9);
            board.GetCell("C8").Candidates.EliminateCandidate(9);
            board.GetCell("H7").Candidates.EliminateCandidate(3);
            board.GetCell("H7").Candidates.EliminateCandidate(5);
            board.GetCell("H7").Candidates.EliminateCandidate(7);
            board.GetCell("H8").Candidates.EliminateCandidate(5);
            board.GetCell("H8").Candidates.EliminateCandidate(7);
            IList<Cell>[] expected = {new List<Cell>(), new List<Cell>() };
            expected[0].Add(board.GetCell("A4"));
            expected[0].Add(board.GetCell("C3"));
            expected[0].Add(board.GetCell("E6"));
            expected[0].Add(board.GetCell("F1"));
            expected[0].Add(board.GetCell("G5"));
            expected[0].Add(board.GetCell("I7"));
            expected[1].Add(board.GetCell("F4"));
            expected[1].Add(board.GetCell("G3"));
            expected[1].Add(board.GetCell("H9"));
            expected[1].Add(board.GetCell("I6"));
            var sut = new Solver(board);

            //act
            List<Cell>[] actual = sut.BuildColoringChains(board.GetCell("A4"), 3);

            //assert
            Assert.That(actual[0].Count, Is.EqualTo(actual[0].Count));
            Assert.That(actual[1].Count, Is.EqualTo(actual[1].Count));
            Assert.That(actual[0].Contains(expected[0][0]));
            Assert.That(actual[0].Contains(expected[0][1]));
            Assert.That(actual[0].Contains(expected[0][2]));
            Assert.That(actual[0].Contains(expected[0][3]));
            Assert.That(actual[0].Contains(expected[0][4]));
            Assert.That(actual[0].Contains(expected[0][5]));
            Assert.That(actual[1].Contains(expected[1][0]));
            Assert.That(actual[1].Contains(expected[1][1]));
            Assert.That(actual[1].Contains(expected[1][2]));
            Assert.That(actual[1].Contains(expected[1][3]));
        }

        #endregion
    }
}
