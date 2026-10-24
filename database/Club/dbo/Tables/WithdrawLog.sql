CREATE TABLE [dbo].[WithdrawLog] (
    [YYYYMMDD]      INT              NOT NULL,
    [ID]            UNIQUEIDENTIFIER NOT NULL,
    [CurrentAmount] BIGINT           NOT NULL,
    [UpdateAmount]  BIGINT           NOT NULL,
    [RemainAmount]  BIGINT           NOT NULL,
    [Type]          TINYINT          NULL,
    [Memo]          VARCHAR (50)     NULL,
    [CreateTime]    DATETIME         CONSTRAINT [DF_WithdrawLog_CreateTime] DEFAULT (getdate()) NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_WithdrawLog]
    ON [dbo].[WithdrawLog]([YYYYMMDD] ASC, [ID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'現有金額', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WithdrawLog', @level2type = N'COLUMN', @level2name = N'CurrentAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提領', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WithdrawLog', @level2type = N'COLUMN', @level2name = N'UpdateAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'帐户馀额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WithdrawLog', @level2type = N'COLUMN', @level2name = N'RemainAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'参考FK.WithdrawType  0: 提現 1: 每日收益結算', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WithdrawLog', @level2type = N'COLUMN', @level2name = N'Type';

