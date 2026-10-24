CREATE TABLE [dbo].[GrandConfig] (
    [ID]             UNIQUEIDENTIFIER CONSTRAINT [DF_GrandConfig_ID] DEFAULT (newid()) NOT NULL,
    [AgentID]        UNIQUEIDENTIFIER NOT NULL,
    [ParentID]       UNIQUEIDENTIFIER NOT NULL,
    [Level]          INT              CONSTRAINT [DF_GrandConfig_Level] DEFAULT ((1)) NULL,
    [GradeName]      NVARCHAR (50)    CONSTRAINT [DF_GrandConfig_GradeName] DEFAULT (N'代理') NULL,
    [DistributeRate] INT              CONSTRAINT [DF_GrandConfig_Rate] DEFAULT ((0)) NOT NULL,
    [UpgradeLimit]   BIGINT           NULL,
    [NewRate]        INT              CONSTRAINT [DF_GrandConfig_NewRate] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_GrandConfig_1] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_GrandConfig]
    ON [dbo].[GrandConfig]([AgentID] ASC, [ParentID] ASC, [Level] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下級代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'ID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下級代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'AgentID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'等級 1:最低 依次往上加', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'Level';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'階級名稱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'GradeName';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'父級代理設定的收益比例', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'DistributeRate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'升級的流水限制', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'UpgradeLimit';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下次變更的比例', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GrandConfig', @level2type = N'COLUMN', @level2name = N'NewRate';

