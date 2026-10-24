CREATE TABLE [Customer].[QuestionType] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [TypeName]        VARCHAR (128) NOT NULL,
    [TypeSort]        VARCHAR (10)  CONSTRAINT [DF_QuestionType_TypeSort] DEFAULT ('') NOT NULL,
    [ChildTypeFileds] VARCHAR (MAX) CONSTRAINT [DF_QuestionType_ChildTypeFileds] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_QuestionType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

