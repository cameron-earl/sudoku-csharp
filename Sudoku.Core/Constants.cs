namespace Sudoku.Core
{
    public static class Constants
    {
        public static int BoardLength = 9;
        public static int TotalCellCount = BoardLength*BoardLength;
        public static int TotalHouseCount = BoardLength*3;

        public enum SolveMethod
        { // List of strategies and order of difficulty taken from paper in Docs folder "Sudoku Creation and Grading"
            Unsolved,
            Provided,
            PlayerInput,
            NakedSingle, // Last unfilled cell in a house
            HiddenSingle, // Only cell in a house with a particular candidate
            NakedPair, // Two cells in the same house have the same two candidates, eliminating those candidates from the rest of the house
            HiddenPair, // Two candidates can only be found in two cells of the same house, eliminating all other candidates in those two cells
            IntersectionRemoval, // All of a certain candidate in a house share another house, eliminating them from the other cells in the second house
            //NakedTriple, // Three cells in the same house have only three candidates between them, eliminating those candidates from the rest of the house
            //HiddenTriple, // Three candidates can only be found in three cells of the same house, eliminating all other candidates in those three cells
            //NakedQuad, // Four cells in the same house have only four candidates between them, eliminating those candidates from the rest of the house
            //HiddenQuad, // Four candidates can only be found in four cells of the same house, eliminating all other candidates in those three cells
            //XWing,
            //SimpleColouring,
            //YWing,
            //SwordFish,
            //XCycle,
            //// ReSharper disable once InconsistentNaming
            //XYChain,
            //ThreeMMedusa,
            //JellyFish,
            //AvoidableRectangle,
            //UniqueRectangle,
            //HiddenUniqueRectangle,
            //// ReSharper disable once InconsistentNaming
            //XYZWing,
            //AlignedPairExclusion,
            //GroupedXCycle,
            //EmptyRectangle,
            //FinnedXWing,
            //FinnedSwordFish,
            //FrankenSwordFish,
            //AlternInferenceChain,
            //DigitForcingChain,
            //CellForcingChain,
            //UnitForcingChain,
            //SuedeCoq,
            //AlmostLockedSet,
            //DeathBlossom,
            //PatternOverlay,
            //QuadForcingChain,
            //Nishio,
            //BowmanBingo
        }
    }
}
