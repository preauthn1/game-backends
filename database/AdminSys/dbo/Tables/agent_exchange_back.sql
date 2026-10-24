CREATE TABLE [dbo].[agent_exchange_back] (
    [id]              INT      IDENTITY (1, 1) NOT NULL,
    [agent_id]        INT      DEFAULT ((0)) NOT NULL,
    [exchange_id]     INT      DEFAULT ((0)) NOT NULL,
    [old_agent_money] INT      DEFAULT ((0)) NOT NULL,
    [new_agent_money] INT      DEFAULT ((0)) NOT NULL,
    [turn_num]        SMALLINT DEFAULT ((1)) NOT NULL,
    [status]          TINYINT  DEFAULT ((0)) NOT NULL,
    [created_at]      DATETIME DEFAULT (getdate()) NULL,
    [turn_status]     TINYINT  DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理回收表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'兑换订单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'exchange_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理旧金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'old_agent_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理新金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'new_agent_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转发次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'turn_num';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态0处理中1成功2失败', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账状态0未完成1已完成', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_exchange_back', @level2type = N'COLUMN', @level2name = N'turn_status';

