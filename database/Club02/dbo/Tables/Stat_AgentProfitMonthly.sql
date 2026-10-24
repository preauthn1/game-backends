CREATE TABLE [dbo].[Stat_AgentProfitMonthly] (
    [YYYYMM] INT              NOT NULL,
    [ID]     UNIQUEIDENTIFIER NOT NULL,
    [Flows]  BIGINT           CONSTRAINT [DF_Stat_IncomeMonthly_flows] DEFAULT ((0)) NOT NULL,
    [Income] INT              CONSTRAINT [DF_Stat_IncomeMonthly_Income] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Stat_AgentProfitMonthly] PRIMARY KEY CLUSTERED ([YYYYMM] ASC, [ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitMonthly', @level2type = N'COLUMN', @level2name = N'ID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'月流水總和', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitMonthly', @level2type = N'COLUMN', @level2name = N'Flows';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'每月收益總和', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Stat_AgentProfitMonthly', @level2type = N'COLUMN', @level2name = N'Income';

