CREATE TABLE [Game].[ForbidChatUser] (
    [UserID]     INT           NOT NULL,
    [Memo]       VARCHAR (128) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_ForbidChatUser] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

