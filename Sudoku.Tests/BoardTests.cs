using System.Net.Mime;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests
{
    [TestFixture]
    public class BoardTests
    {
        private const string BoardString =
            "490600025 800020719 125809043 568092034 700030008 213485067 370961000 002057401 001000306";

        [SetUp]
        public void SetUp()
        {
            var sut = new Board(BoardString);
        }



    }
}
