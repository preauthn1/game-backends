CREATE TABLE [Area].[GameList] (
    [Id]         INT     IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT     NOT NULL,
    [GameID]     INT     NOT NULL,
    [Sort]       INT     NOT NULL,
    [Status]     TINYINT DEFAULT ((0)) NULL,
    [Popular]    TINYINT CONSTRAINT [DF_GameList_Popular] DEFAULT ((0)) NOT NULL,
    [AreaID]     INT     NULL,
    [IconID]     INT     NULL,
    CONSTRAINT [PK_GameList_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_GameList_GameID]
    ON [Area].[GameList]([PlatformID] ASC, [GameID] ASC);

