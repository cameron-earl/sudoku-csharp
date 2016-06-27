using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests
{
    [TestFixture]
    public class ConstantsTests
    {
        #region GetEasiestMove Tests

        [Test]
        public static void ShouldReturnNakedSingle ()
        {
            //arrange
            string expected = Constants.SolvingTechnique.NakedSingle.ToString();
            string unexpected = Constants.SolvingTechnique.HiddenSingle.ToString();

            //act
            string actual = Constants.GetEasiestMove(expected, unexpected);
            //assert
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public static void ShouldNotThrowException()
        {
            //arrange
            string gibberish = "blibberBlabber";

            //act
            //assert
            Assert.DoesNotThrow(() => Constants.GetEasiestMove(gibberish, gibberish));
        }

        [Test]
        public static void ShouldNotReturnUnsolved()
        {
            //arrange
            string gibberish = "blibberBlabber";
            string expected = Constants.SolvingTechnique.NakedSingle.ToString();

            //act
            string actual = Constants.GetEasiestMove(gibberish, expected);

            //assert
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public static void ShouldReturnUnsolved()
        {
            //arrange
            string gibberish = "blibberBlabber";
            string expected = Constants.SolvingTechnique.Unsolved.ToString();

            //act
            string actual = Constants.GetEasiestMove(gibberish, gibberish);
            //assert
            Assert.That(expected, Is.EqualTo(actual));
        }

        #endregion
    }
}
