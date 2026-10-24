CREATE TABLE [Customer].[ReplyContent] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Title]       VARCHAR (64)   NOT NULL,
    [Content]     NVARCHAR (256) NOT NULL,
    [ReplyTypeID] INT            NOT NULL,
    [Sort]        INT            NOT NULL,
    CONSTRAINT [PK_ReplyContent] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'快捷回复内容表', @level0type = N'SCHEMA', @level0name = N'Customer', @level1type = N'TABLE', @level1name = N'ReplyContent';

