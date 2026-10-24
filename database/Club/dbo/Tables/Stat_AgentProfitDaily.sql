CREATE TABLE [dbo].[Stat_AgentProfitDaily] (
    [YYYYMMDD] INT              NOT NULL,
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [Flows]    BIGINT           CONSTRAINT [DF_Stat_IncomeDaily_flows] DEFAULT ((0)) NOT NULL,
    [Rate]     INT              CONSTRAINT [DF_Stat_AgentProfitDaily_Rate] DEFAULT ((0)) NOT NULL,
    [Income]   INT              CONSTRAINT [DF_Stat_IncomeDaily_Income] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Stat_AgentProfitDaily] PRIMARY KEY CLUSTERED ([YYYYMMDD] ASC, [ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitDaily', @level2type = N'COLUMN', @level2name = N'ID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'每天流水總和', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitDaily', @level2type = N'COLUMN', @level2name = N'Flows';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'每天收益總和', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitDaily', @level2type = N'COLUMN', @level2name = N'Income';

