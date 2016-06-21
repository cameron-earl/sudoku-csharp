using System;
using System.Linq;

namespace Sudoku.Core
{
    public class CandidateSet
    {
        private int _solvedValue;

        #region Constructors
        public CandidateSet(int startingValue)
        {
            SolvedValue = startingValue;
            if (startingValue > 0) Candidates[startingValue - 1] = true;
            else
            {
                for (var i = 0; i < Candidates.Length; i++)
                {
                    Candidates[i] = true;
                }
            }
        }

        public CandidateSet(CandidateSet otherSet)
        {
            SolvedValue = otherSet.SolvedValue;
            Array.Copy(otherSet.Candidates, Candidates, otherSet.Count());
        }
        #endregion

        #region Properties
        public bool[] Candidates { get; } = new bool[Constants.BoardLength];

        public int SolvedValue
        {
            get
            {
                if (_solvedValue == 0 && Count() == 1)
                {
                    UpdateSolvedValue();
                }
                return _solvedValue;
            }
            internal set
            {
                if (value < 0 || value > 9)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (value > 0)
                {
                    for (int i = 0; i < Candidates.Length; i++)
                    {
                        Candidates[i] = (i + 1 == value);
                    }
                }

                _solvedValue = value;
            }
        }
        #endregion
        
        #region Methods
        public override string ToString()
        {
            string candidatesStr = "";
            for (int candidate = 1; candidate <= Constants.BoardLength; candidate++)
            {
                if (Candidates[candidate - 1])
                {
                    candidatesStr += $"{candidate}";
                }
            }
            return candidatesStr;
        }

        public int Count()
        {
            int count = 0;
            for (int i = 0; i < Constants.BoardLength; i++)
            {
                if (Candidates[i])
                {
                    count++;
                }
            }
            if (count == 0)
            {
                throw new Exception("A cell was found with zero candidates.");
            }
            return count;
        }

        public bool Contains(int value)
        {
            if (value > Candidates.Length || value < 1)
            {
                return false;
            }
            return Candidates[value - 1];
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var otherSet = (CandidateSet) obj;
            return !Candidates.Where((t, i) => t != otherSet.Candidates[i]).Any();
        }

        public bool EliminateCandidate(int val)
        {
            //Validate move is legal
            if (val < 1 || val > Constants.BoardLength)
            {
                throw new ArgumentOutOfRangeException();
            }
            int index = val - 1;
            if (Count() == 1 && Candidates[index])
            {
                throw new Exception($"Tried to eliminate the last valid candidate in a cell. A previously entered value is likely incorrect.");
            }

            //Remove Candidate
            if (!Candidates[index]) return false;
            Candidates[index] = false;
            Count();
            return true;
        }

        private void UpdateSolvedValue()
        {
            for (int i = 0; i < Candidates.Length; i++)
            {
                if (!Candidates[i]) continue;
                SolvedValue = i + 1;
                return;
            }
            throw new Exception("Something funky happened.");
        }

        public int[] GetCandidateArray()
        {
            int[] candArr = new int[Count()];
            int canditateIndex = 0;
            for (int i = 0; canditateIndex < Candidates.Length && i < candArr.Length; i++)
            {
                while (!Candidates[canditateIndex])
                {
                    canditateIndex++;
                }
                candArr[i] = canditateIndex + 1;
                canditateIndex++;
            }
            return candArr;
        } 
        #endregion

    }
}
