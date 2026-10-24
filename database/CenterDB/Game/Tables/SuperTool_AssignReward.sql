CREATE TABLE [Game].[SuperTool_AssignReward] (
    [LogID]      INT           IDENTITY (1, 1) NOT NULL,
    [GameID]     INT           NOT NULL,
    [FlagID]     INT           NOT NULL,
    [TableNo]    INT           NOT NULL,
    [Complete]   INT           CONSTRAINT [DF_SuperTool_AssignReward_Complete] DEFAULT ((0)) NOT NULL,
    [Result]     VARCHAR (500) NULL,
    [Status]     TINYINT       CONSTRAINT [DF_SuperTool_AssignReward_Status] DEFAULT ((1)) NOT NULL,
    [Operator]   VARCHAR (50)  NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_SuperTool_AssignReward_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_AssignReward] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SuperTool_AssignReward]
    ON [Game].[SuperTool_AssignReward]([GameID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成度', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignReward', @level2type = N'COLUMN', @level2name = N'Complete';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'開獎內容', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignReward', @level2type = N'COLUMN', @level2name = N'Result';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 取消  1 啟用 2 暫停', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignReward', @level2type = N'COLUMN', @level2name = N'Status';

