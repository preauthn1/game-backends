CREATE TABLE [dbo].[agent_plan] (
    [id]                   INT           IDENTITY (1, 1) NOT NULL,
    [total_recharge]       INT           CONSTRAINT [DF__agent_pla__total__66EA454A] DEFAULT ((0)) NOT NULL,
    [single_recharge]      INT           CONSTRAINT [DF__agent_pla__singl__67DE6983] DEFAULT ((0)) NOT NULL,
    [total_agent_recharge] INT           CONSTRAINT [DF__agent_pla__total__68D28DBC] DEFAULT ((0)) NOT NULL,
    [total_pump]           INT           CONSTRAINT [DF__agent_pla__total__69C6B1F5] DEFAULT ((0)) NOT NULL,
    [agents]               NVARCHAR (50) CONSTRAINT [DF__agent_pla__agent__6ABAD62E] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK__agent_pl__3213E83F4CF5691D] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理广告显示方案', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan', @level2type = N'COLUMN', @level2name = N'total_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'单次充值或转入', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan', @level2type = N'COLUMN', @level2name = N'single_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan', @level2type = N'COLUMN', @level2name = N'total_agent_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总抽水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan', @level2type = N'COLUMN', @level2name = N'total_pump';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'优先代理等级', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_plan', @level2type = N'COLUMN', @level2name = N'agents';

