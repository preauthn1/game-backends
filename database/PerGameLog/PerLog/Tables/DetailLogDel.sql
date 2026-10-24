CREATE TABLE [PerLog].[DetailLogDel] (
    [Id]            BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogID]         BIGINT   NOT NULL,
    [ServerID]      INT      NOT NULL,
    [UserID]        INT      NOT NULL,
    [CurrentAmount] BIGINT   NOT NULL,
    [UpdateAmount]  BIGINT   NOT NULL,
    [GameTime]      DATETIME NOT NULL,
    [GameID]        INT      NULL,
    [days]          INT      NULL,
    [TradeID]       BIGINT   DEFAULT ((0)) NOT NULL
) ON [S_Day] ([GameTime]);


GO
CREATE CLUSTERED INDEX [IX_DetailLogDel_GameTime]
    ON [PerLog].[DetailLogDel]([GameTime] ASC)
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IX_DetailLogDel_LogID]
    ON [PerLog].[DetailLogDel]([LogID] ASC)
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IX_DetailLogDel_Days_GameID_ServerID_UserID_TradeID_LogID]
    ON [PerLog].[DetailLogDel]([days] ASC)
    INCLUDE([GameID], [ServerID], [UserID], [TradeID], [LogID], [Id], [CurrentAmount], [UpdateAmount], [GameTime])
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IDX_DetailLogDel_TradeID]
    ON [PerLog].[DetailLogDel]([TradeID] ASC)
    ON [S_Day] ([GameTime]);

