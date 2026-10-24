CREATE TABLE [Game].[CardTypeLog] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [ServerID]    INT          NOT NULL,
    [UserID]      INT          NOT NULL,
    [TypeName]    VARCHAR (32) NOT NULL,
    [MoneyAmount] BIGINT       NOT NULL,
    [CreateTime]  DATETIME     NOT NULL,
    CONSTRAINT [PK_CardTypeLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_CardTypeLog_ServerID_CreateTime]
    ON [Game].[CardTypeLog]([ServerID] ASC, [CreateTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardTypeLog_UserID_CreateTime]
    ON [Game].[CardTypeLog]([UserID] ASC, [CreateTime] ASC);

