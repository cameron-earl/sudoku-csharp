using System;

namespace Sudoku.Core
{
    public class CandidateSet
    {
        private int _solvedValue;

        public CandidateSet(int startingValue)
        {
            SolvedValue = startingValue;
            if (startingValue > 0) Candidates[startingValue - 1] = true;
            else {
                for ( var i = 0; i < Candidates.Length; i++)
                {
                    Candidates[i] = true;
                }
            }
        }

        public bool[] Candidates { get; set; } = new bool[Constants.BoardLength];

        public void EliminateCandidate(int num)
        {
            if (num < 1 || num > Constants.BoardLength)
            {
                throw new ArgumentOutOfRangeException();
            }
            num--;
            Candidates[num] = false;
            CountCandidates();
        }

        public override string ToString()
        {
            var candidatesStr = "";

            for (var row = 0; row < Constants.BoardLength / 3; row++)
            {
                for (var col = 0; col < Constants.BoardLength / 3; col++)
                {
                    var index = row * 3 + col;
                    var num = index + 1;
                    candidatesStr += (Candidates[index]) ? $"{num}" : " ";
                }
                candidatesStr += "\n";
            }

            return candidatesStr;
        }

        public int CountCandidates()
        {
            if (SolvedValue > 0) return 1;

            var count = 0;
            var index = -1;
            for (var i = 0; i < Constants.BoardLength; i++)
            {
                if (Candidates[i])
                {
                    count++;
                    index = i;
                }
            }
            if (count == 1)
            {
                SolvedValue = index + 1;
            }
            return count;
        }

        public int SolvedValue
        {
            get { return _solvedValue; }
            set
            {
                var candidateIndex = value - 1;
                if (value < 0 || value > Constants.BoardLength)
                {
                    throw new ArgumentOutOfRangeException();
                }
                for (var i = 0; i < Candidates.Length; i++)
                {
                    if (value == 0) Candidates[i] = true;
                    else if (i != candidateIndex) Candidates[i] = false;
                }
                _solvedValue = value;
                
            }
        }

        public bool Contains(int value)
        {
            if (value > Candidates.Length || value < 1)
            {
                return false;
            }
            return Candidates[value - 1];
        }
    }
}
