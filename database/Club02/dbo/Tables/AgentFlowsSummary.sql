CREATE TABLE [dbo].[AgentFlowsSummary] (
    [YYYYMMDD] INT              NOT NULL,
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [Flows]    BIGINT           CONSTRAINT [DF_AgentFlows_Flows] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AgentFlows] PRIMARY KEY CLUSTERED ([YYYYMMDD] ASC, [ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理當天獲得的流水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentFlowsSummary', @level2type = N'COLUMN', @level2name = N'Flows';

