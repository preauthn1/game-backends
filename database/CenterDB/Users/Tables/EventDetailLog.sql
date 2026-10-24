CREATE TABLE [Users].[EventDetailLog] (
    [ID]         BIGINT       IDENTITY (1, 1) NOT NULL,
    [EventID]    SMALLINT     NOT NULL,
    [UserID]     INT          NOT NULL,
    [LoginName]  VARCHAR (32) NULL,
    [CreateTime] DATETIME     NOT NULL,
    [GiftCash]   BIGINT       NOT NULL,
    [IPAddress]  VARCHAR (40) NOT NULL,
    CONSTRAINT [PK_EventDetailLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventDetailLog_UserID]
    ON [Users].[EventDetailLog]([UserID] ASC)
    INCLUDE([CreateTime], [GiftCash], [IPAddress]);


GO
CREATE NONCLUSTERED INDEX [IX_EventDetailLog_EventID]
    ON [Users].[EventDetailLog]([EventID] ASC)
    INCLUDE([UserID], [CreateTime], [GiftCash], [IPAddress]);

