-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/09
-- Description:	Slot_LightningGameRecord
-- =============================================

CREATE TABLE [Game].[Slot_LightningGameRecord] (
  [ID] bigint IDENTITY(1,1) NOT NULL,
  [UserID] int,
  [GameID] int,
  [FlagID] int,
  [Data] text,
  PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
  CONSTRAINT [IX_Slot_LightningGameRecord] UNIQUE NONCLUSTERED ([UserID] ASC, [GameID] ASC, [FlagID] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

ALTER TABLE [Game].[Slot_LightningGameRecord] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'玩家ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_LightningGameRecord',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'遊戲ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_LightningGameRecord',
'COLUMN', N'GameID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'房間類型ID',
'SCHEMA', N'Game',
'TABLE', N'Slot_LightningGameRecord',
'COLUMN', N'FlagID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'遊戲資料(自行做字串處理)',
'SCHEMA', N'Game',
'TABLE', N'Slot_LightningGameRecord',
'COLUMN', N'Data'