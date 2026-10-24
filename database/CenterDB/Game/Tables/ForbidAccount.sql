CREATE TABLE [Game].[ForbidAccount] (
    [AccountID]  VARCHAR (50)  NOT NULL,
    [IdType]     TINYINT       NOT NULL,
    [Memo]       VARCHAR (100) NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_ForbidAccount_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ForbidAccount] PRIMARY KEY CLUSTERED ([AccountID] ASC, [IdType] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ForbidAccount]
    ON [Game].[ForbidAccount]([AccountID] ASC, [IdType] ASC, [CreateTime] ASC)
    INCLUDE([Memo]);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 正式帐号(电话) 2支付宝帐号', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'ForbidAccount', @level2type = N'COLUMN', @level2name = N'IdType';

