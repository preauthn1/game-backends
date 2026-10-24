CREATE TABLE [Stat].[GameOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [GameID]       INT      NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_GameOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_GameOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_GameOnLineUserLog_GameID_StatTime]
    ON [Stat].[GameOnLineUserLog]([GameID] ASC, [StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_GameOnLineUserLog_StatTime]
    ON [Stat].[GameOnLineUserLog]([StatTime] ASC)
    INCLUDE([PCCount], [IOSCount], [AndroidCount]);

