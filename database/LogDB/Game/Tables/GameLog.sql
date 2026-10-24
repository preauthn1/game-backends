CREATE TABLE [Game].[GameLog] (
    [LogID]      INT          IDENTITY (1, 1) NOT NULL,
    [UserID]     INT          NOT NULL,
    [ServerID]   INT          NOT NULL,
    [LoginMoney] BIGINT       NOT NULL,
    [Amount]     BIGINT       NOT NULL,
    [TaxAmount]  BIGINT       NOT NULL,
    [WinCount]   INT          NOT NULL,
    [LoseCount]  INT          NOT NULL,
    [DrawCount]  INT          NOT NULL,
    [GameTime]   INT          NOT NULL,
    [IPAddress]  VARCHAR (15) NOT NULL,
    [HDCode]     VARCHAR (64) NOT NULL,
    [HDType]     TINYINT      NOT NULL,
    [LoginTime]  DATETIME     NOT NULL,
    [LogoutTime] DATETIME     NOT NULL,
    [GameID]     INT          NULL,
    [AreaID]     INT          NULL,
    [PlatformID] INT          NULL,
    [TradeID]    BIGINT       DEFAULT ((0)) NOT NULL,
    [FlowAmount] BIGINT       NULL
) ON [S_Data] ([LogoutTime]);


GO
CREATE CLUSTERED INDEX [IUX_GameLog_LogoutTime]
    ON [Game].[GameLog]([LogoutTime] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_LogID]
    ON [Game].[GameLog]([LogID] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_ServerID]
    ON [Game].[GameLog]([ServerID] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_HDCode]
    ON [Game].[GameLog]([HDCode] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_IPAddress]
    ON [Game].[GameLog]([IPAddress] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_UserID_TradeID]
    ON [Game].[GameLog]([UserID] ASC, [TradeID] ASC)
    ON [S_Data] ([LogoutTime]);


GO
CREATE NONCLUSTERED INDEX [IX_GameLog_GameID]
    ON [Game].[GameLog]([GameID] ASC, [LoginTime] ASC)
    INCLUDE([UserID])
    ON [PRIMARY];

