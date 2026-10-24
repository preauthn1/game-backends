CREATE TABLE [dbo].[UserFlowsLog] (
    [LogID]      BIGINT           IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]   INT              NOT NULL,
    [ID]         UNIQUEIDENTIFIER NOT NULL,
    [UserID]     INT              NOT NULL,
    [Class]      TINYINT          NULL,
    [Amount]     BIGINT           NOT NULL,
    [Flows]      BIGINT           CONSTRAINT [DF_CashFlowsDetail_TaxAmount] DEFAULT ((0)) NOT NULL,
    [CreateTime] DATETIME         CONSTRAINT [DF_CashFlowsDetail_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_UserFlowsLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_FlowsDetail]
    ON [dbo].[UserFlowsLog]([YYYYMMDD] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'當日遊戲輸贏流水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsLog', @level2type = N'COLUMN', @level2name = N'Amount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'當日流水(遊戲輸贏流水*2.5%)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsLog', @level2type = N'COLUMN', @level2name = N'Flows';

