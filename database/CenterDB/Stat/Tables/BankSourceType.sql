CREATE TABLE [Stat].[BankSourceType] (
    [SourceID]   INT          NOT NULL,
    [SourceName] VARCHAR (32) NOT NULL,
    [ChangeType] TINYINT      NOT NULL,
    CONSTRAINT [PK_BankSourceType] PRIMARY KEY CLUSTERED ([SourceID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 增加 1 减少 2 流通', @level0type = N'SCHEMA', @level0name = N'Stat', @level1type = N'TABLE', @level1name = N'BankSourceType', @level2type = N'COLUMN', @level2name = N'ChangeType';

