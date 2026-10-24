CREATE TABLE [Notify].[Attr] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [UserID]       INT           NOT NULL,
    [TypeID]       TINYINT       NOT NULL,
    [Param]        BIGINT        NULL,
    [SParam]       VARCHAR (512) NULL,
    [Status]       TINYINT       NOT NULL,
    [CreateTime]   DATETIME      NOT NULL,
    [LastPostTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_Attr] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Attr_CreateTime_Status]
    ON [Notify].[Attr]([CreateTime] ASC, [Status] ASC) WITH (FILLFACTOR = 80);


GO
CREATE NONCLUSTERED INDEX [IX_Attr_LastPostTime]
    ON [Notify].[Attr]([LastPostTime] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 金币 1 ', @level0type = N'SCHEMA', @level0name = N'Notify', @level1type = N'TABLE', @level1name = N'Attr', @level2type = N'COLUMN', @level2name = N'TypeID';

