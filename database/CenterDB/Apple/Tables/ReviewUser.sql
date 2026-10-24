CREATE TABLE [Apple].[ReviewUser] (
    [ID]                 INT          IDENTITY (1, 1) NOT NULL,
    [UserID]             INT          NOT NULL,
    [UserFlag]           TINYINT      CONSTRAINT [DF_ReviewUser_UserFlag] DEFAULT ((0)) NOT NULL,
    [HDCode]             VARCHAR (64) NOT NULL,
    [LastLogonTime]      DATETIME     NULL,
    [LastLogonIPAddress] VARCHAR (30) NULL,
    [CreateTime]         DATETIME     CONSTRAINT [DF_ReviewUser_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ReviewUser] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ReviewUser]
    ON [Apple].[ReviewUser]([UserID] ASC);

