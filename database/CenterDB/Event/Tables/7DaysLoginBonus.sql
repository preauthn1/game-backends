CREATE TABLE [Event].[7DaysLoginBonus](
	[PlatformID] [int] NOT NULL,
	[AreaID] [int] NOT NULL,
	[Day1] [int] NOT NULL,
	[Day2] [int] NOT NULL,
	[Day3] [int] NOT NULL,
	[Day4] [int] NOT NULL,
	[Day5] [int] NOT NULL,
	[Day6] [int] NOT NULL,
	[Day7] [int] NOT NULL,
	[Status] [tinyint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day1]  DEFAULT ((0)) FOR [Day1]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day2]  DEFAULT ((0)) FOR [Day2]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day3]  DEFAULT ((0)) FOR [Day3]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day4]  DEFAULT ((0)) FOR [Day4]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day5]  DEFAULT ((0)) FOR [Day5]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Day6]  DEFAULT ((0)) FOR [Day6]
GO

ALTER TABLE [Event].[7DaysLoginBonus] ADD  CONSTRAINT [DF_Event.˙7DaysLoginBonus_Status]  DEFAULT ((0)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0代表平台預設' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'7DaysLoginBonus', @level2type=N'COLUMN',@level2name=N'PlatformID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0代表渠道預設' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'7DaysLoginBonus', @level2type=N'COLUMN',@level2name=N'AreaID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'贈送金幣數(單位:分)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'7DaysLoginBonus', @level2type=N'COLUMN',@level2name=N'Day1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0: 關, 1: 開' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'7DaysLoginBonus', @level2type=N'COLUMN',@level2name=N'Status'
GO


