USE [CenterDB]
GO

/****** Object:  Table [Event].[7DaysLoginBonus]    Script Date: 2019/7/3 下午 06:14:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
	[Status] [tinyint] NOT NULL,
 CONSTRAINT [PK_7DaysLoginBonus] PRIMARY KEY CLUSTERED 
(
	[PlatformID] ASC,
	[AreaID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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


USE [CenterDB]
GO

INSERT INTO [Event].[7DaysLoginBonus]
           ([PlatformID]
           ,[AreaID]
           ,[Day1]
           ,[Day2]
           ,[Day3]
           ,[Day4]
           ,[Day5]
           ,[Day6]
           ,[Day7]
           ,[Status])
     VALUES
           (0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,1)
GO


