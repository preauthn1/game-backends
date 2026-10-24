CREATE TABLE [Money].[BankLog] (
    [LogID]         INT           IDENTITY (1, 1) NOT NULL,
    [UserID]        INT           NOT NULL,
    [SourceTypeID]  INT           NOT NULL,
    [CashAmount]    BIGINT        NOT NULL,
    [CurrentAmount] BIGINT        NOT NULL,
    [UpdateAmount]  BIGINT        NOT NULL,
    [IPAddress]     VARCHAR (15)  NOT NULL,
    [Memo]          VARCHAR (128) NOT NULL,
    [CreateTime]    DATETIME      NOT NULL,
    [AreaID]        INT           NULL,
    [PlatformID]    INT           NULL
) ON [S_Data] ([CreateTime]);


GO
CREATE CLUSTERED INDEX [ICX_BankLog_CreateTime]
    ON [Money].[BankLog]([CreateTime] ASC) WITH (FILLFACTOR = 80)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_BankLog_SourceTypeID]
    ON [Money].[BankLog]([SourceTypeID] ASC) WITH (FILLFACTOR = 80)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_BankLog_UserID]
    ON [Money].[BankLog]([UserID] ASC) WITH (FILLFACTOR = 80)
    ON [S_Data] ([CreateTime]);

