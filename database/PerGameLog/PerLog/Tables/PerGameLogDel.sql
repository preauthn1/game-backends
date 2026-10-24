CREATE TABLE [PerLog].[PerGameLogDel] (
    [Id]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [ServerID]  INT           NOT NULL,
    [GameData]  VARCHAR (MAX) NULL,
    [TaxAmount] BIGINT        NOT NULL,
    [BeginTime] DATETIME      NOT NULL,
    [EndTime]   DATETIME      NOT NULL,
    [GameID]    INT           NULL,
    [days]      INT           NULL
) ON [S_Day] ([EndTime]);


GO
CREATE CLUSTERED INDEX [IX_PerGameLogDel_GameTime]
    ON [PerLog].[PerGameLogDel]([EndTime] ASC)
    ON [S_Day] ([EndTime]);

