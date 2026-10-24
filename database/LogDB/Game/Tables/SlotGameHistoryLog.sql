CREATE TABLE [Game].[SlotGameHistoryLog] (
    [LogID]         BIGINT        NOT NULL,
    [CreateTime]    DATETIME      NOT NULL,
    [PlatformID]    INT           NOT NULL,
    [AreaID]        INT           NOT NULL,
    [GameID]        INT           NOT NULL,
    [ServerID]      INT           NOT NULL,
    [UserID]        INT           NOT NULL,
    [CurrentAmount] BIGINT        NOT NULL,
    [BetMoney]      BIGINT        NOT NULL,
    [WinMoney]      BIGINT        NOT NULL,
    [FinalScore]    BIGINT        NOT NULL,
    [GameType]      TINYINT       NOT NULL,
    [LogType]       TINYINT       NOT NULL,
    [GameData]      VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_SlotGameHistoryLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_SlotGameHistoryLog_UserID_GameID_CT]
    ON [Game].[SlotGameHistoryLog]([UserID] ASC, [GameID] ASC, [CreateTime] ASC);

