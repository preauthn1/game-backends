CREATE TABLE [Game].[SuperTool_AssignRewardDetail] (
    [LogID]       INT           NOT NULL,
    [Idx]         INT           NOT NULL,
    [Description] VARCHAR (500) NULL,
    [Reward]      VARCHAR (500) NULL,
    [Status]      TINYINT       NOT NULL,
    [CreateTime]  DATETIME      CONSTRAINT [DF_SuperTool_AssignRewardDetail_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_AssignRewardDetail_1] PRIMARY KEY CLUSTERED ([LogID] ASC, [Idx] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SuperTool_AssignRewardDetail]
    ON [Game].[SuperTool_AssignRewardDetail]([LogID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'開獎內容', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignRewardDetail', @level2type = N'COLUMN', @level2name = N'Description';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'開獎結果', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignRewardDetail', @level2type = N'COLUMN', @level2name = N'Reward';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 未完成, 1 己取用, 2 己完成, 3指定開獎', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_AssignRewardDetail', @level2type = N'COLUMN', @level2name = N'Status';

