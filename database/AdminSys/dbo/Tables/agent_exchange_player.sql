CREATE TABLE [dbo].[agent_exchange_player] (
    [id]               INT             IDENTITY (1, 1) NOT NULL,
    [player_id]        INT             NOT NULL,
    [money]            INT             DEFAULT ((0)) NOT NULL,
    [fee]              INT             DEFAULT ((0)) NOT NULL,
    [old_money]        DECIMAL (10, 2) DEFAULT ((0)) NOT NULL,
    [new_money]        DECIMAL (10, 2) DEFAULT ((0)) NOT NULL,
    [old_bank]         DECIMAL (10, 2) DEFAULT ((0)) NOT NULL,
    [new_bank]         DECIMAL (10, 2) DEFAULT ((0)) NOT NULL,
    [ip]               NVARCHAR (20)   DEFAULT ('') NOT NULL,
    [heap_exchange]    INT             DEFAULT ((0)) NOT NULL,
    [heap_recharge]    INT             DEFAULT ((0)) NOT NULL,
    [heap_exchage_req] INT             DEFAULT ((0)) NOT NULL,
    [code]             INT             DEFAULT ((0)) NOT NULL,
    [status]           TINYINT         DEFAULT ((0)) NOT NULL,
    [created_at]       DATETIME        DEFAULT (getdate()) NULL,
    [updated_at]       DATETIME        DEFAULT (getdate()) NULL,
    [alipay_account]   NVARCHAR (100)  DEFAULT ('') NOT NULL,
    [alipay_name]      NVARCHAR (50)   DEFAULT ('') NOT NULL,
    [bank_account]     NVARCHAR (50)   DEFAULT ('') NOT NULL,
    [bank_name]        NVARCHAR (50)   DEFAULT ('') NOT NULL,
    [bank_affiliate]   NVARCHAR (50)   DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家找代理兑换表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提现金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'手续费', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'fee';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提现前金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'old_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提现后金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'new_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提现前银行金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'old_bank';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提现后银行金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'new_bank';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前累计提现', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'heap_exchange';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前累计充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'heap_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'累计申请中兑换', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'heap_exchage_req';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'订单码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态0审核中1成功', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'提交时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'updated_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'支付宝账户', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'alipay_account';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'支付宝名字', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'alipay_name';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'银行账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'bank_account';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'银行姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'bank_name';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'所属银行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_player', @level2type = N'COLUMN', @level2name = N'bank_affiliate';

