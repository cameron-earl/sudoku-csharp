CREATE TABLE [dbo].[Boards]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Puzzle] NVARCHAR(81) NOT NULL, 
	[DateAdded] SMALLDATETIME NOT NULL DEFAULT GETDATE(), 
    [Score] INT NULL, 
    [SolvedValues] NVARCHAR(81) NULL, 
    [HardestMove] NVARCHAR(50) NULL, 
    [TimesPlayed] INT NOT NULL DEFAULT 0, 
    [HardestMoveCount] INT NULL 
)
