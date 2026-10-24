CREATE TABLE [Stat].[MoneySourceLog] (
    [Id]             INT      IDENTITY (1, 1) NOT NULL,
    [SourceID]       INT      NOT NULL,
    [Amount]         BIGINT   NOT NULL,
    [RobotAmount]    BIGINT   CONSTRAINT [DF_MoneySourceLog_RobotAmount] DEFAULT ((0)) NOT NULL,
    [RobotTaxAmount] BIGINT   CONSTRAINT [DF_MoneySourceLog_RobotTaxAmount] DEFAULT ((0)) NOT NULL,
    [TaxAmount]      BIGINT   CONSTRAINT [DF_MoneySourceLog_TaxAmount] DEFAULT ((0)) NOT NULL,
    [StatTime]       DATETIME NOT NULL,
    CONSTRAINT [PK_MoneySourceLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_MoneySourceLog_SourceID_StatTime]
    ON [Stat].[MoneySourceLog]([StatTime] ASC, [SourceID] ASC)
    INCLUDE([Amount], [RobotAmount], [RobotTaxAmount], [TaxAmount]);

