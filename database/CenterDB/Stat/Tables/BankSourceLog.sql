CREATE TABLE [Stat].[BankSourceLog] (
    [Id]       INT      IDENTITY (1, 1) NOT NULL,
    [SourceID] INT      NOT NULL,
    [Amount]   BIGINT   NOT NULL,
    [StatTime] DATETIME NOT NULL,
    CONSTRAINT [PK_BankSourceLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_BankSourceLog_SourceID_StatTime]
    ON [Stat].[BankSourceLog]([SourceID] ASC, [StatTime] ASC);

