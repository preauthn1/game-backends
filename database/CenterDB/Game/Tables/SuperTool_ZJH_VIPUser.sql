CREATE TABLE [Game].[SuperTool_ZJH_VIPUser] (
    [UserID]               INT          NOT NULL,
    [NextType]             INT          NULL,
    [NextWinLose]          INT          NULL,
    [NextWinLoseNum]       INT          NULL,
    [Finish]               INT          NULL,
    [FinishNextWinLoseNum] INT          NULL,
    [NextTable]            INT          NULL,
    [Status]               TINYINT      NULL,
    [Operator]             VARCHAR (50) NOT NULL,
    [CreateTime]           DATETIME     CONSTRAINT [DF_SuperTool_ZJH_VIPUser_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_ZJH_VIPUser] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下局指定牌型', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'NextType';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下局输赢', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'NextWinLose';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下局连续输赢局数', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'NextWinLoseNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否完成下局指定牌型', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'Finish';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否完成下局连续输赢局数', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'FinishNextWinLoseNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下局指定進入桌號', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'NextTable';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'按鈕 0 暂停 1 继续', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ZJH_VIPUser', @level2type = N'COLUMN', @level2name = N'Status';

