CREATE TABLE [Message].[MessageList] (
    [MsgID]      INT            IDENTITY (1, 1) NOT NULL,
    [Title]      VARCHAR (128)  NOT NULL,
    [MsgContent] VARCHAR (1024) NOT NULL,
    [Type]       TINYINT        NOT NULL,
    [FromUserID] INT            NULL,
    [Sort]       INT            NOT NULL,
    [CreateTime] DATETIME       NOT NULL,
    [MsgSeq]     BIGINT         NULL,
    CONSTRAINT [PK_MessageList] PRIMARY KEY NONCLUSTERED ([MsgID] ASC) ON [PRIMARY]
) ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_MessageList_CreateTime]
    ON [Message].[MessageList]([CreateTime] ASC)
    ON [S_Data] ([CreateTime]);

