CREATE TABLE [Users].[UserLogonLog] (
    [LogID]      BIGINT       IDENTITY (1, 1) NOT NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_UserLogonLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    [UserID]     INT          NULL,
    [HDType]     TINYINT      NULL,
    [ServerID]   INT          CONSTRAINT [DF_UserLogonLog_ServerID] DEFAULT ((0)) NULL,
    [AreaID]     INT          CONSTRAINT [DF_UserLogonLog_AreaID] DEFAULT ((0)) NULL,
    [IPAddress]  VARCHAR (40) NULL,
    [UserLevel]  INT          CONSTRAINT [DF_UserLogonLog_UserLevel] DEFAULT ((0)) NULL,
    [PlatFormID] INT          NULL,
    [HDCode]     VARCHAR (64) NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_UserLogonLog_CreateTime_UserID]
    ON [Users].[UserLogonLog]([CreateTime] ASC, [UserID] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_UserLogonLog_HDType_IPAddress]
    ON [Users].[UserLogonLog]([HDType] ASC, [IPAddress] ASC);

