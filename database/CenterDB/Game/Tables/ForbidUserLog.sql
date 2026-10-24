CREATE TABLE [Game].[ForbidUserLog] (
    [LogID]      INT           IDENTITY (1, 1) NOT NULL,
    [UserID]     INT           NOT NULL,
    [ForbidType] TINYINT       NOT NULL,
    [CashAmount] BIGINT        NOT NULL,
    [BankAmount] BIGINT        NOT NULL,
    [Memo]       VARCHAR (128) NOT NULL,
    [IPAddress]  VARCHAR (15)  NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_ForbidUserLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ForbidUserLog_CreateTime]
    ON [Game].[ForbidUserLog]([CreateTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ForbidUserLog_UserID]
    ON [Game].[ForbidUserLog]([UserID] ASC);

