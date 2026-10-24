CREATE TABLE [dbo].[FishJackpotLog] (
    [SeqID]           BIGINT          IDENTITY (1, 1) NOT NULL,
    [ServerID]        INT             NOT NULL,
    [ShotID]          BIGINT          DEFAULT ((0)) NOT NULL,
    [ShotNum]         INT             DEFAULT ((0)) NOT NULL,
    [ShotBet]         BIGINT          DEFAULT ((0)) NOT NULL,
    [FishMoney]       BIGINT          DEFAULT ((0)) NOT NULL,
    [Win]             BIGINT          DEFAULT ((0)) NOT NULL,
    [CommissionMoney] INT             DEFAULT ((0)) NOT NULL,
    [LineNum]         INT             DEFAULT ((0)) NOT NULL,
    [LineBase]        INT             DEFAULT ((0)) NOT NULL,
    [LineUp]          INT             DEFAULT ((0)) NOT NULL,
    [LineDown]        INT             DEFAULT ((0)) NOT NULL,
    [PowerBase]       INT             DEFAULT ((0)) NOT NULL,
    [PowerAdd]        INT             DEFAULT ((0)) NOT NULL,
    [Power]           INT             DEFAULT ((0)) NOT NULL,
    [AvgBetTotal]     DECIMAL (13, 3) DEFAULT ((0)) NULL,
    [ShotTotal]       BIGINT          DEFAULT ((0)) NOT NULL,
    [ShotBetCalc]     BIGINT          DEFAULT ((0)) NOT NULL,
    [FishMoneyTotal]  BIGINT          DEFAULT ((0)) NOT NULL,
    [WinTotal]        BIGINT          DEFAULT ((0)) NOT NULL,
    [ShotBetTotal]    BIGINT          DEFAULT ((0)) NOT NULL,
    [CommissionTotal] BIGINT          DEFAULT ((0)) NOT NULL,
    [InitCalc]        INT             DEFAULT ((0)) NOT NULL,
    [CurrDate]        DATETIME2 (7)   DEFAULT (getdate()) NULL,
    CONSTRAINT [PK__FishJack__7AF8B187155B1B70] PRIMARY KEY CLUSTERED ([SeqID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [index_fishjackpotlog_serverid]
    ON [dbo].[FishJackpotLog]([ServerID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'系统奖池日志，（每个ServerID的最大SeqID不能删除，在服务器启动的时候会读取最近一条数据）add by kevin', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'自增ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'SeqID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'服务器ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ServerID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'系统总发射子弹数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前子弹量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前下注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotBet';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前掉落金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'FishMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前赢取金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'Win';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前抽水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'CommissionMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'水位下标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'LineNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'水位线', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'LineBase';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'水位上限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'LineUp';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'水位下限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'LineDown';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'初始子弹威力', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'PowerBase';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'增加子弹威力', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'PowerAdd';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前威力', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'Power';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总平均子弹倍数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'AvgBetTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总子弹量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总扣除抽水后的下注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotBetCalc';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总掉落金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'FishMoneyTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总输赢金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'WinTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总下注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'ShotBetTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总抽水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'CommissionTotal';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'即将初始化水位(1:即将初始化, 0:历史数据计算水位)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'InitCalc';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishJackpotLog', @level2type = N'COLUMN', @level2name = N'CurrDate';

