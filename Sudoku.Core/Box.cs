namespace Sudoku.Core
{
    internal class Box : House
    {

        public Box(int houseNumber)
        {
            HouseNumber = houseNumber;
        }

        public override HouseType MyHouseType { get; } = HouseType.Box;

        public override string ToString()
        {
            int houseIndex = HouseNumber - 1;
            bool isTop = houseIndex/3 == 0;
            bool isBottom = houseIndex/3 == 2;
            bool isRight = houseIndex%3 == 2;
            bool isLeft = houseIndex%3 == 0;
            char topLeft = (isLeft)?((isTop)?'╔':'╠'):((isTop)?'╦':'╬');
            char topRight = (isRight) ? ((isTop) ? '╗' : '╣') : ((isTop) ? '╦' : '╬');
            char bottomLeft = (isLeft) ? ((isBottom) ? '╚' : '╠') : ((isBottom) ? '╩' : '╬');
            char bottomRight = (isRight) ? ((isBottom) ? '╝' : '╣') : ((isBottom) ? '╩' : '╬');
            char leftSide = (isLeft)?'╠':'╬';
            char rightSide = (isRight)?'╣':'╬';
            char top = (isTop)?'╦': '╬';
            char bottom = (isBottom)?'╩': '╬';

            string topEdge = $"{topLeft}═══{top}═══{top}═══{topRight}\n";
            string divider = $"{leftSide}───┼───┼───{rightSide}\n";
            string bottomEdge = $"{bottomLeft}═══{bottom}═══{bottom}═══{bottomRight}\n";
            string boxString = topEdge;
            for (int row = 0; row < Constants.BoardLength/3; row++)
            {
                boxString += "║ ";
                for (int col = 0; col < Constants.BoardLength/3; col++)
                {
                    boxString += $"{Cells[row*3 + col]} ";
                    boxString += (col < 2) ? "│ " : "║\n";
                }
                boxString += (row < 2) ? divider : bottomEdge;
            }
            return boxString;
        }
    }
}