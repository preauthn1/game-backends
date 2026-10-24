CREATE TABLE [Event].[FirstDeposit](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PlatformID] [int] NOT NULL,
	[AreaID] [int] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[DepositLimit] [bigint] NOT NULL,
	[WinAmount] [bigint] NOT NULL,
	[ReturnAmount] [bigint] NOT NULL,
	[Status] [tinyint] NULL,
 CONSTRAINT [PK_FirstDeposit] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_FirstDeposit_1] UNIQUE NONCLUSTERED 
(
	[PlatformID] ASC,
	[AreaID] ASC,
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Event].[FirstDeposit] ADD  CONSTRAINT [DF_FirstDeposit_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO

ALTER TABLE [Event].[FirstDeposit] ADD  CONSTRAINT [DF_FirstDeposit_DepositLimit]  DEFAULT ((0)) FOR [DepositLimit]
GO

ALTER TABLE [Event].[FirstDeposit] ADD  CONSTRAINT [DF_FirstDeposit_WinAmount]  DEFAULT ((0)) FOR [WinAmount]
GO

ALTER TABLE [Event].[FirstDeposit] ADD  CONSTRAINT [DF_FirstDeposit_ReturnAmount]  DEFAULT ((0)) FOR [ReturnAmount]
GO

ALTER TABLE [Event].[FirstDeposit] ADD  CONSTRAINT [DF_FirstDeposit_Status]  DEFAULT ((0)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'單筆充值金額條件(單位: 分)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDeposit', @level2type=N'COLUMN',@level2name=N'DepositLimit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'累计赢钱额度(單位:分)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDeposit', @level2type=N'COLUMN',@level2name=N'WinAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'返利金額(單位: 分)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDeposit', @level2type=N'COLUMN',@level2name=N'ReturnAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'活動狀態(0: 關, 1:開)' , @level0type=N'SCHEMA',@level0name=N'Event', @level1type=N'TABLE',@level1name=N'FirstDeposit', @level2type=N'COLUMN',@level2name=N'Status'
GO

