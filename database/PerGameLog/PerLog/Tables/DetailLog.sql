CREATE TABLE [PerLog].[DetailLog] (
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
CREATE CLUSTERED INDEX [IX_DetailLog_GameTime]
    ON [PerLog].[DetailLog]([GameTime] ASC)
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IX_DetailLog_LogID]
    ON [PerLog].[DetailLog]([LogID] ASC)
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IX_DetailLog_Days_GameID_ServerID_UserID_TradeID_LogID]
    ON [PerLog].[DetailLog]([days] ASC)
    INCLUDE([GameID], [ServerID], [UserID], [TradeID], [LogID], [Id], [CurrentAmount], [UpdateAmount], [GameTime])
    ON [S_Day] ([GameTime]);


GO
CREATE NONCLUSTERED INDEX [IDX_DetailLog_TradeID]
    ON [PerLog].[DetailLog]([TradeID] ASC)
    ON [S_Day] ([GameTime]);

