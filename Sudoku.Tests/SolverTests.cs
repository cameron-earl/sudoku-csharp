using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
