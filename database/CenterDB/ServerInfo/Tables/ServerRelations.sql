CREATE TABLE [ServerInfo].[ServerRelations] (
    [ID]       INT IDENTITY (1, 1) NOT NULL,
    [AreaID]   INT NOT NULL,
    [GameID]   INT NOT NULL,
    [ServerID] INT NOT NULL,
    [FlagID]   INT NOT NULL,
    [Status]   BIT NOT NULL,
    CONSTRAINT [PK_ServerRelations] PRIMARY KEY CLUSTERED ([ID] ASC)
);

