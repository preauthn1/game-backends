CREATE TABLE [Customer].[QuestionHandleRecords] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [QsId]           INT           NOT NULL,
    [QsHandleStatus] TINYINT       NOT NULL,
    [Memo]           VARCHAR (MAX) NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [CreateUserName] VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_QuestionHandleRecord] PRIMARY KEY CLUSTERED ([Id] ASC)
);

