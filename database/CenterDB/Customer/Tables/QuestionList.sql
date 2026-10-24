CREATE TABLE [Customer].[QuestionList] (
    [Id]                     INT           IDENTITY (1, 1) NOT NULL,
    [QsTypeId]               INT           NOT NULL,
    [Title]                  VARCHAR (128) NOT NULL,
    [ClientType]             TINYINT       NOT NULL,
    [Origin]                 TINYINT       NOT NULL,
    [ChildTypeFiledsContent] VARCHAR (MAX) NULL,
    [ContactName]            VARCHAR (20)  NULL,
    [ContactPhone]           VARCHAR (128) NULL,
    [HandleStatus]           TINYINT       NOT NULL,
    [CreateUserName]         VARCHAR (20)  NOT NULL,
    [CreateTime]             DATETIME      NOT NULL,
    [Memo]                   VARCHAR (MAX) NULL,
    [SiteID]                 INT           NOT NULL,
    [UserID]                 INT           NULL,
    CONSTRAINT [PK_QuestionList] PRIMARY KEY CLUSTERED ([Id] ASC)
);

