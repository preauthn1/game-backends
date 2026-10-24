CREATE TABLE [Users].[EventLog] (
    [EventID]    SMALLINT     NOT NULL,
    [UserID]     INT          NOT NULL,
    [LoginName]  VARCHAR (32) NULL,
    [Updater]    VARCHAR (20) NULL,
    [UpdateTime] DATETIME     NULL,
    [GrantTimes] TINYINT      NULL,
    CONSTRAINT [PK_EventLog_1] PRIMARY KEY CLUSTERED ([EventID] ASC, [UserID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventLog_UpdateTime]
    ON [Users].[EventLog]([EventID] ASC, [LoginName] ASC, [UpdateTime] ASC);

