using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Core
{
    using System;

    public class SolvingException : Exception
    {
        public SolvingException()
        {
        }

        public SolvingException(string message)
            : base(message)
        {
        }

        public SolvingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
