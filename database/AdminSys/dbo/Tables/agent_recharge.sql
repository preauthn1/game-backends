CREATE TABLE [dbo].[agent_recharge] (
    [id]         INT      IDENTITY (1, 1) NOT NULL,
    [manage_id]  INT      NOT NULL,
    [agent_id]   INT      NOT NULL,
    [num]        INT      NOT NULL,
    [created_at] DATETIME DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'旧表暂废弃', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'管理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge', @level2type = N'COLUMN', @level2name = N'manage_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'充值数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_recharge', @level2type = N'COLUMN', @level2name = N'num';

