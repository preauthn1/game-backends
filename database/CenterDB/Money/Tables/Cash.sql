CREATE TABLE [Money].[Cash](
	[UserID] [int] NOT NULL,
	[Amount] [bigint] NOT NULL,
	[Code] [varchar](32) NOT NULL,
	[CouponAmount] [bigint] NULL,
 CONSTRAINT [PK_Cash] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

ALTER TABLE [Money].[Cash] ADD  CONSTRAINT [DF_Cash_Code]  DEFAULT ('') FOR [Code]
GO

ALTER TABLE [Money].[Cash] ADD  DEFAULT ((0)) FOR [CouponAmount]
GO