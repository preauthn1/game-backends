CREATE TABLE [dbo].[agent_player] (
    [player_id]      INT NOT NULL,
    [total_recharge] INT DEFAULT ((0)) NOT NULL,
    [total_exchage]  INT DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([player_id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理玩家表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家总代理充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player', @level2type = N'COLUMN', @level2name = N'total_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家总代理兑换', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_player', @level2type = N'COLUMN', @level2name = N'total_exchage';

