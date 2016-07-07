SELECT b.Puzzle, b.HardestMove, b.HardestMoveCount 
	FROM (
		SELECT  Puzzle, HardestMove, HardestMoveCount, ROW_NUMBER()
		over (Partition BY HardestMove
			ORDER BY HardestMoveCount DESC ) AS Rank
		FROM dbo.Boards
		) b 
		JOIN dbo.Techniques t ON b.HardestMove = t.Technique
		WHERE Rank <= 5
ORDER BY t.Id