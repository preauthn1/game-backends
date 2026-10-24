CREATE TABLE [Money].[ConvertLog] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [UserID]         INT           NOT NULL,
    [Amount]         BIGINT        NOT NULL,
    [TaxAmount]      BIGINT        CONSTRAINT [DF_ConvertLog_TaxAmount] DEFAULT ((0)) NOT NULL,
    [RealTaxAmount]  BIGINT        CONSTRAINT [DF_ConvertLog_RealTaxAmount] DEFAULT ((0)) NOT NULL,
    [Status]         TINYINT       NOT NULL,
    [ConvertType]    TINYINT       NOT NULL,
    [RealName]       VARCHAR (64)  NOT NULL,
    [Account]        VARCHAR (64)  NOT NULL,
    [BankName]       VARCHAR (50)  NULL,
    [CreateIP]       VARCHAR (15)  NOT NULL,
    [TransID]        VARCHAR (64)  NULL,
    [TransData]      VARCHAR (MAX) NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [LastUpdateTime] DATETIME      NOT NULL,
    [CheckTime]      DATETIME      NOT NULL,
    [PlatformID]     INT           NULL,
    [AreaID]         INT           NULL,
    [BankAmount]     BIGINT        NULL,
    [CashAmount]     BIGINT        NULL,
    [Operator]       VARCHAR (16)  NULL,
    [Memo]           VARCHAR (200) NULL,
    [GWID]           INT           CONSTRAINT [DF_ConvertLog_GWID] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_ConvertLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_ConvertLog_UserID]
    ON [Money].[ConvertLog]([UserID] ASC, [Amount] ASC, [CreateTime] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'兑换记录', @level0type = N'SCHEMA', @level0name = N'Money', @level1type = N'TABLE', @level1name = N'ConvertLog';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'真实手续费', @level0type = N'SCHEMA', @level0name = N'Money', @level1type = N'TABLE', @level1name = N'ConvertLog', @level2type = N'COLUMN', @level2name = N'RealTaxAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 提出兑换 1 审核中 2 审核通过 3 已打款 4 订单异常 5 异常帐号 6 黑名单 7 账户错误 8 拒绝兑换 9 疑似洗钱 10 补单完成 11 退还金币 98 人工处理 99 排程处理', @level0type = N'SCHEMA', @level0name = N'Money', @level1type = N'TABLE', @level1name = N'ConvertLog', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'支付寶1, 銀聯卡2', @level0type = N'SCHEMA', @level0name = N'Money', @level1type = N'TABLE', @level1name = N'ConvertLog', @level2type = N'COLUMN', @level2name = N'ConvertType';

