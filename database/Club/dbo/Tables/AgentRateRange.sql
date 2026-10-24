CREATE TABLE [dbo].[AgentRateRange] (
    [ID]              UNIQUEIDENTIFIER NOT NULL,
    [DistributeRate]  INT              NOT NULL,
    [Class1RateRange] INT              CONSTRAINT [DF_AgentRateRange_Class1RateRange] DEFAULT ((0)) NULL,
    [Class2RateRange] INT              CONSTRAINT [DF_AgentRateRange_Class2RateRange] DEFAULT ((0)) NULL,
    [Class3RateRange] INT              CONSTRAINT [DF_AgentRateRange_Class3RateRange_1] DEFAULT ((0)) NULL,
    [Class4RateRange] INT              CONSTRAINT [DF_AgentRateRange_Class4RateRange_1] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_GrandRange] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'總代理(渠道代理) 收益趴數', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentRateRange', @level2type = N'COLUMN', @level2name = N'DistributeRate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'一級代理收益啪數最大值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentRateRange', @level2type = N'COLUMN', @level2name = N'Class1RateRange';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'二級代理收益啪數最大值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentRateRange', @level2type = N'COLUMN', @level2name = N'Class2RateRange';

