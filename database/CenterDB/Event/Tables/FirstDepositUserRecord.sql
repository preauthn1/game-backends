CREATE TABLE [Event].[FirstDepositUserRecord](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[EventID] [int] NOT NULL,
	[JoinTime] [datetime] NOT NULL,
	[DepositAmount] [bigint] NOT NULL,
	[WinAmount] [bigint] NOT NULL,
	[Flag] [tinyint] NULL,
	[CollectTime] [datetime] NULL,
	[CollectAmount] [bigint] NULL,
 CONSTRAINT [PK_FirstDepositUserRecord] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_FirstDepositUserRecord] UNIQUE NONCLUSTERED 
(
	[UserID] ASC,
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Event].[FirstDepositUserRecord] ADD  CONSTRAINT [DF_FirstDepositUserRecord_EventID]  DEFAULT ((0)) FOR [EventID]
GO

ALTER TABLE [Event].[FirstDepositUserRecord] ADD  CONSTRAINT [DF_FirstDepositUserRecord_JoinTime]  DEFAULT ((0)) FOR [JoinTime]
GO

ALTER TABLE [Event].[FirstDepositUserRecord] ADD  CONSTRAINT [DF_FirstDepositUserRecord_DepositAmount]  DEFAULT ((0)) FOR [DepositAmount]
GO

ALTER TABLE [Event].[FirstDepositUserRecord] ADD  CONSTRAINT [DF_FirstDepositUserRecord_WinAmount]  DEFAULT ((0)) FOR [WinAmount]
GO

ALTER TABLE [Event].[FirstDepositUserRecord] ADD  CONSTRAINT [DF_FirstDepositUserRecord_Flag]  DEFAULT ((0)) FOR [Flag]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'對應活動ID' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDepositUserRecord', @level2type=N'COLUMN',@level2name=N'EventID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加入時間' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDepositUserRecord', @level2type=N'COLUMN',@level2name=N'JoinTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'當次單筆充值金額' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDepositUserRecord', @level2type=N'COLUMN',@level2name=N'DepositAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'累計贏錢額度(單位:分)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDepositUserRecord', @level2type=N'COLUMN',@level2name=N'WinAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'領取時間' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDepositUserRecord', @level2type=N'COLUMN',@level2name=N'CollectTime'
GO

