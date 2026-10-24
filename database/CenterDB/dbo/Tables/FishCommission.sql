CREATE TABLE [dbo].[FishCommission] (
    [SeqID]      BIGINT   IDENTITY (1, 1) NOT NULL,
    [ServerID]   INT      NOT NULL,
    [ShotNum]    INT      DEFAULT ((0)) NOT NULL,
    [Win]        BIGINT   DEFAULT ((0)) NOT NULL,
    [Commission] INT      DEFAULT ((0)) NOT NULL,
    [CurrDate]   DATETIME DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK__FishComm__7AF8B1875FB337D6] PRIMARY KEY CLUSTERED ([SeqID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_FishCommission_CurrDate]
    ON [dbo].[FishCommission]([CurrDate] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'自增ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'SeqID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ServerID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'ServerID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'子弹数（计算抽水子弹数）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'ShotNum';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'系统当前子弹数赢取金币', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'Win';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'抽水额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'Commission';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'当前时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FishCommission', @level2type = N'COLUMN', @level2name = N'CurrDate';

