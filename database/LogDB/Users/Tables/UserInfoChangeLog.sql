CREATE TABLE [Users].[UserInfoChangeLog] (
    [LogID]      INT            IDENTITY (1, 1) NOT NULL,
    [UserID]     INT            NOT NULL,
    [ChangeType] TINYINT        NOT NULL,
    [OldData]    VARCHAR (1024) NOT NULL,
    [NewData]    VARCHAR (1024) NOT NULL,
    [IPAddress]  VARCHAR (15)   NOT NULL,
    [Memo]       VARCHAR (128)  NOT NULL,
    [WriteTime]  DATETIME       NOT NULL,
    CONSTRAINT [PK_UserInfoChangeLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_UserInfoChangeLog_UserID_WriteTime]
    ON [Users].[UserInfoChangeLog]([UserID] ASC, [WriteTime] ASC);

