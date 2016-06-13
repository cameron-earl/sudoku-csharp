namespace Sudoku.Core
{
    public class Row : House
    {

        public Row(int houseNumber)
        {
            HouseNumber = houseNumber;
        }

        public override string ToString()
        {
            var rowString = "";

            for (var col = 0; col < Constants.BoardLength; col++)
            {
                if (col == 0) rowString += $"{(char)('A' + HouseNumber - 1)} ║ ";
                if (col == 3 || col == 6)
                {
                    rowString += "║ ";
                }
                else if (col != 0)
                {
                    rowString += "│ ";
                }
                rowString += $"{Cells[col]} ";
                if (col == 8) rowString += "║\n";
            }

            return rowString;
        }
    }
}