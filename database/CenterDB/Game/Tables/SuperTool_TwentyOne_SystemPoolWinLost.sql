CREATE TABLE [Game].[SuperTool_TwentyOne_SystemPoolWinLost] (
    [PoolDate]   DATETIME NOT NULL,
    [FlagID]     INT      NOT NULL,
    [BetMoney]   BIGINT   CONSTRAINT [DF_SuperTool_TwentyOne_SystemPoolWinLost_BetMoney] DEFAULT ((0)) NOT NULL,
    [BetCounts]  BIGINT   CONSTRAINT [DF_SuperTool_TwentyOne_SystemPoolWinLost_BetCounts] DEFAULT ((0)) NOT NULL,
    [PoolAmount] BIGINT   NOT NULL,
    CONSTRAINT [PK_SuperTool_TwentyOne_SystemPoolWinLost] PRIMARY KEY CLUSTERED ([PoolDate] ASC, [FlagID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'紀錄日期', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_TwentyOne_SystemPoolWinLost', @level2type = N'COLUMN', @level2name = N'PoolDate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下注金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_TwentyOne_SystemPoolWinLost', @level2type = N'COLUMN', @level2name = N'BetMoney';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'下注總次數', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_TwentyOne_SystemPoolWinLost', @level2type = N'COLUMN', @level2name = N'BetCounts';

