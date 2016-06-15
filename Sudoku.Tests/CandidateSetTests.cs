using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Sudoku.Core;
using Assert = NUnit.Framework.Assert;

namespace Sudoku.Tests
{
    [TestFixture]
    public class CandidateSetTests
    {
        #region EqualsTests

        [Test]
        public void ShouldBeEqual()
        {
            //arrange
            var cell1 = new Cell(1, 0);
            var cell2 = new Cell(2, 0);
            for (int i = 2; i <= 9; i++)
            {
                cell1.Candidates.EliminateCandidate(i);
                cell2.Candidates.EliminateCandidate(i);
            }
            //act
            bool actual = cell1.Candidates.Equals(cell2.Candidates);
            //assert
            Assert.That(actual, Is.EqualTo(true));
        }
        #endregion
    }
}
