SELECT HardestMove, COUNT(HardestMove) AS NumberOfMoves 
FROM dbo.Boards 
GROUP BY HardestMove 
ORDER BY NumberOfMoves DESC