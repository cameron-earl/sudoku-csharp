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

    }
}
