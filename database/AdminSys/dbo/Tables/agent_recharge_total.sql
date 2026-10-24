CREATE TABLE [dbo].[agent_recharge_total] (
    [id]             INT            IDENTITY (1, 1) NOT NULL,
    [manage_id]      INT            NOT NULL,
    [total_agent_id] INT            NOT NULL,
    [num]            INT            NOT NULL,
    [created_at]     DATETIME       DEFAULT (getdate()) NULL,
    [old_money]      INT            DEFAULT ((0)) NOT NULL,
    [new_money]      INT            DEFAULT ((0)) NOT NULL,
    [ip]             NVARCHAR (20)  DEFAULT ('') NOT NULL,
    [area]           NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [device]         NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [remark]         NVARCHAR (255) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理充值表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'管理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'manage_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'total_agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'充值数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'num';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'充值前金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'old_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账后金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'new_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'IP地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作地区', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'area';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'终端设备', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'device';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge_total', @level2type = N'COLUMN', @level2name = N'remark';

