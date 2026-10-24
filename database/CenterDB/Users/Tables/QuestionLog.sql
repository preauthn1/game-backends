CREATE TABLE [Users].[QuestionLog] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT             CONSTRAINT [DF_QuestionLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [UserID]       INT             NOT NULL,
    [Question]     VARCHAR (512)   NOT NULL,
    [QuestionType] TINYINT         CONSTRAINT [DF_QuestionLog_QuestionType] DEFAULT ((0)) NOT NULL,
    [Status]       TINYINT         NOT NULL,
    [IPAddress]    VARCHAR (15)    NOT NULL,
    [HandleTime]   DATETIME        NULL,
    [HandlePerson] VARCHAR (32)    NULL,
    [ReplyContent] NVARCHAR (2048) NULL,
    [UserEmail]    VARCHAR (50)    NULL,
    [CreateTime]   DATETIME        NOT NULL,
    [UploadImage]  VARCHAR (50)    CONSTRAINT [DF_QuestionLog_UploadImage] DEFAULT ('') NULL,
    [Operator]     INT             NULL
) ON [S_Data] ([CreateTime]);


GO
CREATE CLUSTERED INDEX [IX_QuestionLog_CreateTime]
    ON [Users].[QuestionLog]([CreateTime] ASC)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_QuestionLog_Id]
    ON [Users].[QuestionLog]([Id] ASC)
    INCLUDE([Status], [CreateTime], [UserID], [HandlePerson])
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_QuestionLog_UserID]
    ON [Users].[QuestionLog]([UserID] ASC)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_QuestionLog_QT_STS_HP_INCLUDE]
    ON [Users].[QuestionLog]([QuestionType] ASC, [Status] ASC, [CreateTime] ASC, [HandlePerson] ASC)
    INCLUDE([Id], [PlatformID], [UserID], [Question], [IPAddress], [HandleTime], [ReplyContent], [UploadImage])
    ON [S_Data] ([CreateTime]);

