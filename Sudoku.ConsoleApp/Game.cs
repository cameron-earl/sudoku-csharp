using System;
using System.Text.RegularExpressions;
using Sudoku.Core;
using static System.Console;

namespace Sudoku.ConsoleApp
{
    internal class Game
    {
        private const bool Testing = true;

        private ConsoleColor _labelColor = ConsoleColor.DarkGray;
        private ConsoleColor _boldFrameColor = Testing ? ConsoleColor.Black : ConsoleColor.Gray;
        private ConsoleColor _minorFrameColor = Testing ? ConsoleColor.Black : ConsoleColor.DarkGray;
        private ConsoleColor _providedValueColor = ConsoleColor.White;
        private ConsoleColor _playerInputColor = ConsoleColor.Gray;
        private ConsoleColor _solvedValueColor = ConsoleColor.Green;
        internal const int ConsoleWindowHeight = 33;
        internal const int MenuPosition = 20;

        public Game(Board board)
        {
            Board = board;
            Solver = new Solver(board);
        }

        public Solver Solver { get; set; }

        public Board Board { get; set; }

        #region Methods

        public void SetupWindow()
        {
            Clear();
            WindowHeight = ConsoleWindowHeight;
            SetCursorPosition(0,0);
            PrintBoardFrame();
        }

        public void Play()
        {
            SetupWindow();

            MainMenu();

        }

        public void PrintBoardFrame()
        {
            SetCursorPosition(0, 0);

            ForegroundColor = _labelColor;
            WriteLine("    1   2   3   4   5   6   7   8   9");
            ForegroundColor = _boldFrameColor;
            WriteLine("  ╔═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╗");

            for (var row = 0; row < Constants.BoardLength; row++)
            {
                if (row == 3 || row == 6)
                {
                    ForegroundColor = _boldFrameColor;
                    WriteLine("  ╠═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╣");
                }
                PrintRow(row + 1);
                if (row % 3 != 2)
                {
                    PrintMixedFrameRow();
                }
            }
            ForegroundColor = _boldFrameColor;
            WriteLine("  ╚═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╝\n");
            ResetColor();
        }

        public void PrintAllCells()
        {
            for (int cell = 1; cell <= Constants.TotalCellCount; cell++)
            {
                if (Board.Cells[cell - 1].HasChanged)
                {
                    PrintCell(cell);
                    Board.Cells[cell - 1].HasChanged = false;
                }
                
            }
            ResetColor();
        }

        public void PrintCell(int cellId)
        {
            Cell cell = Board.GetCell(cellId);
            SetCursorPosition(CursorColForCell(cellId), CursorRowForCell(cellId));
            Constants.SolvingTechnique solvingTechnique = cell.SolvingTechnique;
            switch (solvingTechnique)
            {
                case Constants.SolvingTechnique.Provided:
                    ForegroundColor = _providedValueColor;
                    break;
                case Constants.SolvingTechnique.PlayerInput:
                    ForegroundColor = _playerInputColor;
                    break;
                case Constants.SolvingTechnique.NakedSingle:
                    ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                default:
                    ForegroundColor = _solvedValueColor;
                    break;
            }
            Write(cell);

            
        }

        #endregion

        #region Private Methods

        private void MainMenu()
        {
            
            bool done = false;
            do
            {
                PrepareMenu();
                Menu.PrintHeading("Please enter the letter of an option:");
                WriteLine("  A. Input a Value");
                WriteLine("  B. Solve Easiest Move");
                WriteLine("  C. Solve the Board (as much as possible)");
                WriteLine("  D. Print candidates for each cell");
                WriteLine("  E. Check board for validity");
                WriteLine("  X. Exit program");

                const string regexStr = "[a-ex]";
                char choice = Menu.GetCharacterInput(new Regex(regexStr));
                switch (choice)
                {
                    case 'a':
                        ChangeCellValue();
                        break;
                    case 'b':
                        if (!Solver.SolveEasiestMove()) PrintUnsolvableMessage();
                        break;
                    case 'c':
                        bool changed = true;
                        while (changed)
                        {
                            changed = Solver.SolveEasiestMove();
                            PrintAllCells();
                        }
                        if (!Board.IsSolved()) PrintUnsolvableMessage();
                        break;
                    case 'd':
                        PrintCandidatesForBoard();
                        break;
                    case 'e':
                        WriteLine(Board.IsValid());
                        ReadKey();
                        break;
                    case 'x':
                        done = true;
                        break;
                    default:
                        PrintMessage("Something went wrong, try again.");
                        break;
                }
            } while (!done && !Board.IsSolved());

            if (Board.IsSolved()) PrintMessage($"Congratulations! Sudoku solved!\n{Solver.MoveCountsToString()}");
        }

        private void PrintCandidatesForBoard()
        {
            PrintMessage(Board.CandidatesToString());
        }

        private void PrintUnsolvableMessage()
        {
            Board.IsCorrectlySolved();
            if (Board.IsProvenInvalid)
            {
                PrintMessage($"Somewhere along the line a bad value was chosen. The board is invalid.");
                Menu.PrintHeading($"Reset?");
                if (Menu.GetYesNoInput())
                {
                    Board.ResetBoard();
                    Solver = new Solver(Board);
                }
            }
            else
            {
                PrintMessage($"If you can solve this, you're smarter than I am.\n{Solver.MoveCountsToString()}");
            }
            
            
        }

        private void PrintMessage(string message)
        {
            PrintAllCells();
            PrepareMenu();
            WriteLine(message);
            ReadKey();
        }

        private void DisplayCandidatesForCell()
        {
            string coord = GetCellCoordinateInput();
            if (coord == "Q")
            {
                return;
            }

            Cell cell = Board.GetCell(coord);
            WriteLine(cell.Candidates);
            ReadKey();
        }


        private void ChangeCellValue()
        {
            string coord = GetCellCoordinateInput();
            if (coord == "Q")
            {
                return;
            }

            Cell cell = Board.GetCell(coord);

            int newValue = GetValueInput();
            if (newValue == -1)
            {
                return;
            }
            Board.SetCellValue(cell.CellId,newValue,Constants.SolvingTechnique.PlayerInput);
        }

        private int GetValueInput()
        {
            PrepareMenu();
            Menu.PrintHeading("What should the new value be?");
            WriteLine("\t(enter 0 to erase or -1 to cancel)");
            var input = Menu.GetIntInput(-1, Constants.BoardLength);
            return input;
        }

        private string GetCellCoordinateInput()
        {
            bool isValid = false;
            string input = "";
            string error = "";
            while (!isValid)
            {
                isValid = true;
                input = "";
                PrepareMenu();
                Menu.PrintHeading(error + "Please enter a cell coordinate. (e.g. \"A7\")");
                WriteLine("Type Q to quit.");
                input += ReadLine()?.ToUpper();
                if (input.StartsWith("Q")) return "Q";
                var cleanPattern = new Regex("[^A-I0-9]");
                input = cleanPattern.Replace(input, "");
                if (input.Length > 2) input = input.Substring(0, 2);
                var matchPattern = new Regex("[A-I][0-9]");
                if (!matchPattern.IsMatch(input))
                {
                    error = "Input invalid. Match example format.";
                    isValid = false;
                    continue;
                }
                var cell = Board.GetCell(input);
                if (cell.SolvingTechnique == Constants.SolvingTechnique.Provided)
                {
                    error = "Cannot change a provided cell value!";
                    isValid = false;
                }

            }
            return input;
        }

        private void PrepareMenu()
        {
            SetCursorPosition(0, 0);
            PrintAllCells();
            ClearScreen(MenuPosition);
            SetCursorPosition(0, MenuPosition);
        }

        private static void ClearScreen(int startingRow)
        {
            for (int row = startingRow; row <= startingRow + 50; row++)
            {
                SetCursorPosition(0, row);
                WriteLine(new string(' ', WindowWidth));
            }
            SetCursorPosition(0, 0);
            SetCursorPosition(0, startingRow);
        }

        private void PrintMixedFrameRow()
        {
            ForegroundColor = _boldFrameColor;
            Write("  ╠");
            ForegroundColor = _minorFrameColor;
            Write("───┼───┼───");
            ForegroundColor = _boldFrameColor;
            Write("╬");
            ForegroundColor = _minorFrameColor;
            Write("───┼───┼───");
            ForegroundColor = _boldFrameColor;
            Write("╬");
            ForegroundColor = _minorFrameColor;
            Write("───┼───┼───");
            ForegroundColor = _boldFrameColor;
            WriteLine("╣");
            ResetColor();
        }

        private void PrintRow(int i)
        {
            for (var col = 0; col < Constants.BoardLength; col++)
            {
                if (col == 0)
                {
                    PrintRowLabel(i);
                    PrintMajorFrameVertical();
                }
                if (col == 3 || col == 6)
                {
                    PrintMajorFrameVertical();
                }
                else if (col != 0)
                {
                    PrintMinorFrameVertical();
                }
                SetCursorPosition(CursorLeft + 2, CursorTop);
                if (col == 8)
                {
                    PrintMajorFrameVertical();
                    WriteLine();
                }
            }
        }

        private void PrintMinorFrameVertical()
        {
            ForegroundColor = _minorFrameColor;
            Write("│ ");
        }

        private void PrintMajorFrameVertical()
        {
            ForegroundColor = _boldFrameColor;
            Write("║ ");
        }

        private void PrintRowLabel(int i)
        {
            ForegroundColor = _labelColor;
            Write((char)('@' + i) + " ");
        }

        private static int CursorRowForCell(int cell)
        {
            return Cell.GetCellRow(cell) * 2;
        }

        private static int CursorColForCell(int cell)
        {
            int col = Cell.GetCellCol(cell);
            return (col * 4);
        }

        #endregion
    }
}
