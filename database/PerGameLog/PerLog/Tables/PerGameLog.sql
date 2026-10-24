CREATE TABLE [PerLog].[PerGameLog] (
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
CREATE CLUSTERED INDEX [IUX_PerGameLog_GameTime]
    ON [PerLog].[PerGameLog]([EndTime] ASC)
    ON [S_Day] ([EndTime]);


GO
CREATE NONCLUSTERED INDEX [IX_PerGameLog_Days_GameID_ServerID_DeskID]
    ON [PerLog].[PerGameLog]([days] ASC)
    INCLUDE([GameID], [ServerID])
    ON [S_Day] ([EndTime]);

