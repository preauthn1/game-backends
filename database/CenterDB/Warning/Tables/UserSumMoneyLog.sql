CREATE TABLE [Warning].[UserSumMoneyLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [UserID]     INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [CreateTime] DATETIME NOT NULL,
    CONSTRAINT [PK_UserSumMoneyLog] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 80)
);


GO
CREATE NONCLUSTERED INDEX [IX_UserSumMoneyLog_CreateTime]
    ON [Warning].[UserSumMoneyLog]([CreateTime] ASC);

