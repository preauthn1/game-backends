CREATE TABLE [dbo].[agent_report_day] (
    [id]        INT             IDENTITY (1, 1) NOT NULL,
    [day]       DATE            NOT NULL,
    [agent_id]  INT             NOT NULL,
    [recharge]  INT             DEFAULT ((0)) NOT NULL,
    [circulate] INT             DEFAULT ((0)) NOT NULL,
    [exchange]  INT             DEFAULT ((0)) NOT NULL,
    [back]      INT             DEFAULT ((0)) NOT NULL,
    [sale]      INT             DEFAULT ((0)) NOT NULL,
    [profit]    DECIMAL (10, 2) DEFAULT ((0)) NOT NULL,
    [timeout]   INT             DEFAULT ((0)) NOT NULL,
    [back_num]  INT             DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理日报', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'day';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理充值金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'流通金额 ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'circulate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理兑换', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'exchange';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理回收', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'back';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理卖出金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'sale';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理利润', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'profit';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'超时订单', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'timeout';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'成功回收订单数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_report_day', @level2type = N'COLUMN', @level2name = N'back_num';

