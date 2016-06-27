--SELECT HardestMove, COUNT(HardestMove) AS NumberOfMoves 
--FROM dbo.Boards 
--GROUP BY HardestMove 
--ORDER BY NumberOfMoves DESC

SELECT Puzzle FROM dbo.Boards
WHERE HardestMove='BowmanBingo'