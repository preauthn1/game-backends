CREATE TABLE [dbo].[FishUserMoneyLog] (
    [SeqID]        INT           IDENTITY (1, 1) NOT NULL,
    [UserID]       INT           DEFAULT ((0)) NULL,
    [ServerID]     INT           DEFAULT ((0)) NULL,
    [Exp]          INT           DEFAULT ((0)) NULL,
    [OnlineTime]   INT           DEFAULT ((0)) NULL,
    [WinMoney]     BIGINT        DEFAULT ((0)) NULL,
    [LoseMoney]    BIGINT        DEFAULT ((0)) NULL,
    [ShotNum]      INT           DEFAULT ((0)) NULL,
    [MissMoneyLib] INT           DEFAULT ((0)) NULL,
    [CreateDate]   DATETIME2 (7) DEFAULT (getdate()) NULL,
    CONSTRAINT [PK__FishUser__7AF8B18708B54D69] PRIMARY KEY CLUSTERED ([SeqID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'SeqID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'UserID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'服务器ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'ServerID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'经验', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'Exp';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'在线时长', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'OnlineTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'赢取金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'WinMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'消耗金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'LoseMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'子弹数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'ShotNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Miss库', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'MissMoneyLib';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserMoneyLog', @level2type = N'COLUMN', @level2name = N'CreateDate';

