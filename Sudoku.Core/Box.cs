namespace Sudoku.Core
{
    internal class Box : House
    {

        public Box(int houseNumber)
        {
            HouseNumber = houseNumber;
        }

        public override string ToString()
        {
            var houseIndex = HouseNumber - 1;
            var isTop = houseIndex/3 == 0;
            bool isBottom = houseIndex/3 == 2;
            bool isRight = houseIndex%3 == 2;
            bool isLeft = houseIndex%3 == 0;
            var topLeft = (isLeft)?((isTop)?'╔':'╠'):((isTop)?'╦':'╬');
            var topRight = (isRight) ? ((isTop) ? '╗' : '╣') : ((isTop) ? '╦' : '╬');
            var bottomLeft = (isLeft) ? ((isBottom) ? '╚' : '╠') : ((isBottom) ? '╩' : '╬');
            var bottomRight = (isRight) ? ((isBottom) ? '╝' : '╣') : ((isBottom) ? '╩' : '╬');
            var leftSide = (isLeft)?'╠':'╬';
            var rightSide = (isRight)?'╣':'╬';
            var top = (isTop)?'╦': '╬';
            var bottom = (isBottom)?'╩': '╬';

            var topEdge = $"{topLeft}═══{top}═══{top}═══{topRight}\n";
            var divider = $"{leftSide}───┼───┼───{rightSide}\n";
            var bottomEdge = $"{bottomLeft}═══{bottom}═══{bottom}═══{bottomRight}\n";
            var boxString = topEdge;
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