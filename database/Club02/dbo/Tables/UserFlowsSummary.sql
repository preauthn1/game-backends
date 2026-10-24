CREATE TABLE [dbo].[UserFlowsSummary] (
    [YYYYMMDD] INT              NOT NULL,
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [UserID]   INT              NOT NULL,
    [Amount]   BIGINT           NOT NULL,
    [Flows]    BIGINT           NOT NULL,
    CONSTRAINT [PK_UserFlowsSummary] PRIMARY KEY CLUSTERED ([YYYYMMDD] ASC, [ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家或代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsSummary', @level2type = N'COLUMN', @level2name = N'ID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'遊戲ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsSummary', @level2type = N'COLUMN', @level2name = N'UserID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'當日遊戲輸贏總流水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsSummary', @level2type = N'COLUMN', @level2name = N'Amount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'當日總流水(遊戲輸贏流水*2.5%)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserFlowsSummary', @level2type = N'COLUMN', @level2name = N'Flows';

