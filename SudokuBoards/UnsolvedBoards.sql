﻿CREATE TABLE [dbo].[UnsolvedBoards]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
    [Puzzle] NVARCHAR(81) NOT NULL, 
    [DateAdded] SMALLDATETIME NOT NULL DEFAULT GETDATE()
)