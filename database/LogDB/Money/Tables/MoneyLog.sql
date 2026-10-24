CREATE TABLE [Money].[MoneyLog] (
    [LogID]         INT           IDENTITY (1, 1) NOT NULL,
    [UserID]        INT           NOT NULL,
    [SourceTypeID]  INT           NOT NULL,
    [CurrentAmount] BIGINT        NOT NULL,
    [UpdateAmount]  BIGINT        NOT NULL,
    [TaxAmount]     BIGINT        CONSTRAINT [DF_MoneyLog_TaxAmount] DEFAULT ((0)) NOT NULL,
    [IPAddress]     VARCHAR (15)  NOT NULL,
    [Memo]          VARCHAR (128) NOT NULL,
    [CreateTime]    DATETIME      NOT NULL,
    [AreaID]        INT           NULL,
    [PlatformID]    INT           NULL,
    [TradeID]       BIGINT        DEFAULT ((0)) NOT NULL,
    [FlowAmount]    BIGINT        CONSTRAINT [DF_MoneyLog_FlowAmount] DEFAULT ((0)) NULL
) ON [S_Data] ([CreateTime]);


GO
CREATE CLUSTERED INDEX [IX_MoneyLog_CreateTime]
    ON [Money].[MoneyLog]([CreateTime] ASC)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_MoneyLog_SourceTypeID_UserID_TradeID]
    ON [Money].[MoneyLog]([SourceTypeID] ASC, [UserID] ASC, [TradeID] ASC)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_MoneyLog_STID_PID_INCLUDE]
    ON [Money].[MoneyLog]([SourceTypeID] ASC, [PlatformID] ASC)
    INCLUDE([LogID], [UserID], [AreaID], [IPAddress], [CreateTime])
    ON [S_Data] ([CreateTime]);

