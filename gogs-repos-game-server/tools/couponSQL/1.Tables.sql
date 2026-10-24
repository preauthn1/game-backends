
alter table [LogDB].[Game].[GameLog]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [LogDB].[Money].[MoneyLog]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [CenterDB].[Money].[Cash]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [CenterDB].[Users].[UserInfo]
add SumCouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [PerGameLog].[PerLog].[DetailLog]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [LogDB].[Stat].[StatisticsUserAmount_Day]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES,CouponCount BIGINT null DEFAULT 0 WITH VALUES

alter table [LogDB].[Stat].[StatisticsMoneySourceLog_New]
add CouponAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [LogDB].[Stat].[StatisticsMoneySourceLog_New]
add FlowAmount BIGINT null DEFAULT 0 WITH VALUES

alter table [CenterDB].[Game].[Slot_FreeSpinRecord]
add IsCoupon TINYINT null DEFAULT 0 WITH VALUES


USE [Club02]
GO

CREATE TABLE [dbo].[CouponCode](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CouponCode] [char](12) NOT NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_CouponCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IDX_CouponCode_Code] ON [dbo].[CouponCode]
(
	[CouponCode] ASC,
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CouponUserData](
	[CouponCode] [char](12) NOT NULL,
	[PlatformID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[MultiUser] [int] NOT NULL,
	[MultiCount] [int] NOT NULL,
	[Amount] [bigint] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[UserID] [int] NULL,
	[UseCount] [int] NULL,
	[UseDate] [datetime] NULL,
	[UpdateTime] [datetime] NULL,
 CONSTRAINT [PK_CouponUserData] PRIMARY KEY CLUSTERED 
(
	[CouponCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IDX_CouponUserData_PlatformID_INCLOUD] ON [dbo].[CouponUserData]
(
	[PlatformID] ASC
)
INCLUDE ( 	[CouponCode],
	[Status]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CouponMultiUserLog](
	[LogID] [bigint] IDENTITY(1,1) NOT NULL,
	[CouponCode] [char](12) NOT NULL,
	[UserID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_CouponMultiUserLog] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IDX_CouponMultiUserLog_CouponCode_UserID] ON [dbo].[CouponMultiUserLog]
(
	[CouponCode] ASC,
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
