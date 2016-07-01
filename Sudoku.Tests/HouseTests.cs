using System.Linq;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests
{
    [TestFixture]
    public class HouseTests
    {
        #region MyRegion ContainsTests
        [Test]
        public void ShouldContainValue()
        {
            //arrange
            int[] values = Enumerable.Range(0,Constants.BoardLength).ToArray();
            var sut = new Row(1);
            for (int i = 0; i < 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            const bool expected = true;
            const int testValue = 8;
            //act
            bool actual = sut.Contains(testValue);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldNotContainValue()
        {
            //arrange
            int[] values = Enumerable.Range(0, Constants.BoardLength).ToArray();
            var sut = new Row(1);
            for (int i = 0; i < 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            const bool expected = false;
            const int testValue = 9;
            //act
            bool actual = sut.Contains(testValue);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldNotContainNegativeValue()
        {
            //arrange
            int[] values = Enumerable.Range(0, Constants.BoardLength).ToArray();
            var sut = new Row(1);
            for (int i = 0; i < 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            const bool expected = false;
            const int testValue = -1;
            //act
            bool actual = sut.Contains(testValue);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldNotContainReallyHighValue()
        {
            //arrange
            int[] values = Enumerable.Range(0, Constants.BoardLength).ToArray();
            var sut = new Row(1);
            for (int i = 0; i < 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            const bool expected = false;
            const int testValue = 10;
            //act
            bool actual = sut.Contains(testValue);
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }
        #endregion

        #region UpdateCandidatesTests

        [Test]
        public void ShouldUpdateAllCandidatesCorrectly()
        {
            //arrange
            var values = new[] { 0, 0, 2, 3, 4, 5, 6, 7, 8 };
            var sut = new Row(1);
            for (int i = 0; i< 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            var expected = new[] { "19", "19", "2", "3", "4", "5", "6", "7", "8" };

            //act
            sut.UpdateCandidates();
            
            var actual = new string[9];
            for (int i = 0; i < sut.Cells.Count; i++)
            {
                actual[i] += sut.Cells[i].Candidates.ToString();
            }
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldUpdateThisCandidateCorrectly()
        {
            //arrange
            var values = new[] { 0, 0, 0, 3, 4, 5, 6, 7, 8 };
            var sut = new Row(1);
            for (int i = 0; i < 9; i++)
            {
                sut.Cells.Add(new Cell(i + 1, values[i]));
            }
            var expected = new[] { "19", "19", "2", "3", "4", "5", "6", "7", "8" };

            //act
            sut.UpdateCandidates();
            sut.Cells[2].Value = 2;
            sut.UpdateCandidates(2);

            var actual = new string[9];
            for (int i = 0; i < sut.Cells.Count; i++)
            {
                actual[i] += sut.Cells[i].Candidates.ToString();
            }
            //assert
            Assert.That(actual, Is.EqualTo(expected));
        }


        #endregion
    }


}
