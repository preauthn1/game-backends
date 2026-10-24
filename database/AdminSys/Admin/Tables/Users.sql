CREATE TABLE [Admin].[Users] (
    [UserID]             INT          IDENTITY (1, 1) NOT NULL,
    [LoginName]          VARCHAR (32) NOT NULL,
    [Password]           VARCHAR (32) NOT NULL,
    [Status]             TINYINT      NOT NULL,
    [Phone]              VARCHAR (15) NULL,
    [PhoneCode]          VARCHAR (15) NULL,
    [PhoneCodeTime]      DATETIME     NULL,
    [CreateTime]         DATETIME     NOT NULL,
    [LastLogonTime]      DATETIME     NOT NULL,
    [LastLogonIPAddress] VARCHAR (15) NOT NULL,
    [UserType]           TINYINT      CONSTRAINT [DF_Users_UserType] DEFAULT ((0)) NOT NULL,
    [Displayname]        VARCHAR (20) CONSTRAINT [DF_Users_Displayname] DEFAULT ('') NOT NULL,
    [TGChatid]           INT          CONSTRAINT [DF_Users_TGChatid] DEFAULT ((0)) NOT NULL,
    [SecretKey]          INT          CONSTRAINT [DF_Users_SecretKey] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_Users_LoginName]
    ON [Admin].[Users]([LoginName] ASC);

