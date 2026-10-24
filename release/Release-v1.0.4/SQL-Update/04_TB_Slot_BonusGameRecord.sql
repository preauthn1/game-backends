-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/30
-- Description:	Slot_BonusGameRecord
-- =============================================

CREATE TABLE [Game].[Slot_BonusGameRecord] (
  [ID] bigint IDENTITY(1,1) NOT NULL,
  [UserID] int,
  [GameID] int,
  [FlagID] int,
  [OriginID] int,
  [TurnID] int,
  [Location] int,
  [BetMoney] int,
  [BetRate] int,
  [Data] text,
  PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
  CONSTRAINT [IX_Slot_BonusGameRecord] UNIQUE NONCLUSTERED ([UserID] ASC, [GameID] ASC, [FlagID] ASC, [OriginID] ASC, [TurnID] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

ALTER TABLE [Game].[Slot_BonusGameRecord] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'玩家ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'遊戲ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'GameID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'房間類型ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'FlagID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'規則ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'OriginID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'第幾次選擇',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'TurnID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'選擇位置',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'Location'
GO

EXEC sp_addextendedproperty
'MS_Description', N'下注金額',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'BetMoney'
GO

EXEC sp_addextendedproperty
'MS_Description', N'下注倍率',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'BetRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'遊戲資料(自行做字串處理)',
'SCHEMA', N'Game',
'TABLE', N'Slot_BonusGameRecord',
'COLUMN', N'Data'