CREATE TABLE [dbo].[AgentProfitLog] (
    [LogID]      BIGINT           NOT NULL,
    [YYYYMMDD]   INT              NOT NULL,
    [ID]         UNIQUEIDENTIFIER NOT NULL,
    [Flows]      BIGINT           NOT NULL,
    [Class]      TINYINT          NULL,
    [ConfigRate] INT              NULL,
    [Rate]       INT              NULL,
    [Income]     BIGINT           CONSTRAINT [DF_AgentProfitLog_Income] DEFAULT ((0)) NULL,
    [Level]      TINYINT          NULL,
    [CreateTime] DATETIME         CONSTRAINT [DF_AgentProfitLog_CreateTime] DEFAULT (getdate()) NOT NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理id', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentProfitLog', @level2type = N'COLUMN', @level2name = N'ID';

