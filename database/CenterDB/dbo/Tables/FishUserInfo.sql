CREATE TABLE [dbo].[FishUserInfo] (
    [UserID]         INT           NOT NULL,
    [MaxWeaponLevel] INT           DEFAULT ((0)) NOT NULL,
    [Exp]            INT           DEFAULT ((0)) NOT NULL,
    [OnlineTime]     INT           DEFAULT ((0)) NOT NULL,
    [HisMaxMoney]    BIGINT        DEFAULT ((0)) NOT NULL,
    [TotalWinMoney]  BIGINT        DEFAULT ((0)) NOT NULL,
    [TotalLoseMoney] BIGINT        DEFAULT ((0)) NOT NULL,
    [DayWinMoney]    BIGINT        DEFAULT ((0)) NOT NULL,
    [DayLoseMoney]   BIGINT        DEFAULT ((0)) NOT NULL,
    [TotalShotNum]   BIGINT        DEFAULT ((0)) NOT NULL,
    [MissMoneyLib]   INT           DEFAULT ((0)) NOT NULL,
    [LastLoginTime]  DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK__FishUser__1788CCAC08012052] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'捕鱼用户数据(add by kevin)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用户', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'UserID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'最大炮倍率', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'MaxWeaponLevel';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'捕鱼经验值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'Exp';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'捕鱼在线时长', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'OnlineTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'历史最大金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'HisMaxMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总共捕鱼掉落金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'TotalWinMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总共捕鱼消耗金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'TotalLoseMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当天掉落金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'DayWinMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当天消耗金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'DayLoseMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总共发射子弹次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'TotalShotNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Miss库', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishUserInfo', @level2type = N'COLUMN', @level2name = N'MissMoneyLib';

