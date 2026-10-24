CREATE TABLE [Agent].[Users] (
    [AgentID]       INT          IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (32) NOT NULL,
    [WXNo]          VARCHAR (20) NOT NULL,
    [QQ]            VARCHAR (20) NOT NULL,
    [UserID]        INT          NOT NULL,
    [GroupName]     VARCHAR (32) NOT NULL,
    [IsShow]        BIT          NOT NULL,
    [LoginName]     VARCHAR (32) NOT NULL,
    [AdminSysID]    INT          NOT NULL,
    [LastLoginIP]   VARCHAR (15) NOT NULL,
    [LastLoginTime] DATETIME     NOT NULL,
    [PlatformID]    INT          CONSTRAINT [DF_Users_PlatformID] DEFAULT ((1)) NOT NULL,
    [CreateTime]    DATETIME     NOT NULL
);

