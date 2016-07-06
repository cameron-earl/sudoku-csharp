--SELECT Puzzle, HardestMove, HardestMoveCount FROM dbo.Boards b
--JOIN dbo.Techniques t ON b.HardestMove = t.Technique
--WHERE b.Id in (
--	select top 5 Id
--	from dbo.Boards bb
--	where bb.HardestMove = b.HardestMove
--	order by bb.HardestMoveCount DESC
--)
--ORDER BY t.Id, b.HardestMoveCount DESC

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