CREATE TABLE [Stat].[SumMoneyLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [CashAmount] BIGINT   NOT NULL,
    [BankAmount] BIGINT   NOT NULL,
    [SumIn]      BIGINT   NOT NULL,
    [SumOut]     BIGINT   NOT NULL,
    [StatTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_SumMoneyLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SumMoneyLog_StatTime]
    ON [Stat].[SumMoneyLog]([StatTime] ASC);

