namespace Sudoku.Core
{
    internal class Column : House
    {

        public Column(int houseNumber)
        {
            HouseNumber = houseNumber;
        }
        
        public override HouseType MyHouseType { get; } = HouseType.Column;

        public override string ToString()
        {
            var colString = $"{HouseNumber}\n";
            for (var i = 0; i < Constants.BoardLength; i++)
            {
                colString += (i%3 != 0) ? "─\n" : "═\n";
                colString += $"{Cells[i]}\n";
            }
            colString += "═\n";
            return colString;
        }
    }
}