CREATE TABLE [Stat].[StatisticsMoneySourceLog] (
    [ID]             INT      IDENTITY (1, 1) NOT NULL,
    [SourceTypeID]   INT      NOT NULL,
    [Amount]         BIGINT   CONSTRAINT [DF_MoneySourceLog_Amount] DEFAULT ((0)) NOT NULL,
    [RobotAmount]    BIGINT   CONSTRAINT [DF_MoneySourceLog_RobotAmount] DEFAULT ((0)) NOT NULL,
    [RobotTaxAmount] BIGINT   CONSTRAINT [DF_MoneySourceLog_RobotTaxAmount] DEFAULT ((0)) NOT NULL,
    [TaxAmount]      BIGINT   CONSTRAINT [DF_MoneySourceLog_TaxAmount] DEFAULT ((0)) NOT NULL,
    [StatTime]       DATETIME NOT NULL,
    CONSTRAINT [PK_StatisticsMoneySourceLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_StatisticsMoneySourceLog_StatTime]
    ON [Stat].[StatisticsMoneySourceLog]([StatTime] ASC);

