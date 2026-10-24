CREATE TABLE [dbo].[agent_player_recharge] (
    [id]             INT      IDENTITY (1, 1) NOT NULL,
    [agent_id]       INT      NOT NULL,
    [player_id]      INT      NOT NULL,
    [money]          INT      NOT NULL,
    [created_at]     DATETIME DEFAULT (getdate()) NULL,
    [status]         TINYINT  DEFAULT ((0)) NOT NULL,
    [agent_money_id] INT      DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理给玩家充值明细', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'充值金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态1成功', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理金额明细ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player_recharge', @level2type = N'COLUMN', @level2name = N'agent_money_id';

