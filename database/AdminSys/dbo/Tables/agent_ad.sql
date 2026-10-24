CREATE TABLE [dbo].[agent_ad] (
    [id]       INT      IDENTITY (1, 1) NOT NULL,
    [agent_id] INT      NOT NULL,
    [plan]     INT      NOT NULL,
    [clock]    DATETIME NULL,
    [status]   TINYINT  DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_ad', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'方案', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_ad', @level2type = N'COLUMN', @level2name = N'plan';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'打卡时间精确到分', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_ad', @level2type = N'COLUMN', @level2name = N'clock';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'广告显示状态0排队1显示中', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_ad', @level2type = N'COLUMN', @level2name = N'status';

