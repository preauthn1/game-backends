USE [CenterDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA [Payment]
    AUTHORIZATION [dbo];
GO

CREATE TABLE [Payment].[BankCodeType](
	[BankCode] [nvarchar](50) NULL,
	[TypeID] [int] NULL,
	[SourceTypeID] [int] NULL
) ON [PRIMARY]
GO

USE [CenterDB]
GO

INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('JDPAY_QR',5,11010)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('QQ_H5',4,11003)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('QQ_QR',4,11003)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('WEIXIN_H5',2,11002)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('WEIXIN_QR',21,11002)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('ZHIFUBAO_H5',1,11001)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('ZHIFUBAO_QR',11,11001)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('JDPAY_H5 ',5,11010)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('EXPRESSPAY_H5',3,11004)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('UNIONPAY_QR',3,11004)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('UNIONPAY_ONLINE_BANK',3,11004)
INSERT INTO [Payment].[BankCodeType] ([BankCode],[TypeID],[SourceTypeID])
VALUES ('QUICKPASS_QR',7,11012)
GO


CREATE TABLE [Payment].[MerchantInfo](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GWID] [bigint] NOT NULL,
	[InfoData] [nvarchar](max) NOT NULL,
	[Uuid] [nvarchar](200) NULL,
	[MerchantStatus] [tinyint] NULL,
	[Status] [tinyint] NOT NULL,
	[CreateTime] [datetime] NULL,
	[LastUpdateTime] [datetime] NULL,
 CONSTRAINT [PK_Payment_MerchantInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [Payment].[PayGateway](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GatewayCode] [varchar](20) NOT NULL,
	[GatewayName] [varchar](20) NOT NULL,
	[MerchantInfo] [int] NOT NULL,
	[SupportPayin] [bit] NOT NULL,
	[SupportPayout] [bit] NOT NULL,
	[ReviseAmount] [bit] NOT NULL,
	[BankCodes] [nvarchar](max) NULL,
	[Active] [tinyint] NULL,
	[Status] [tinyint] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastUpdateTime] [datetime] NULL,
 CONSTRAINT [PK_Payment_PayGateway] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Payment].[PayGateway] ADD  CONSTRAINT [DF__PayGatewa__Merch__3E88198C]  DEFAULT ((0)) FOR [MerchantInfo]
GO

ALTER TABLE [Payment].[PayGateway] ADD  CONSTRAINT [DF__PayGatewa__Suppo__3F7C3DC5]  DEFAULT ('FALSE') FOR [SupportPayin]
GO

ALTER TABLE [Payment].[PayGateway] ADD  CONSTRAINT [DF__PayGatewa__Suppo__407061FE]  DEFAULT ('FALSE') FOR [SupportPayout]
GO

ALTER TABLE [Payment].[PayGateway] ADD  CONSTRAINT [DF__PayGatewa__Revis__41648637]  DEFAULT ('FALSE') FOR [ReviseAmount]
GO

ALTER TABLE [Payment].[PayGateway] ADD  CONSTRAINT [DF__PayGatewa__Statu__4258AA70]  DEFAULT ((0)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'第三方金流代碼' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'GatewayCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'第三方金流名稱' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'GatewayName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所需商務號資訊' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'MerchantInfo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支援代收交易' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'SupportPayin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支援代付交易' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'SupportPayout'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否會調整金額' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'ReviseAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支援之銀行代碼' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayGateway', @level2type=N'COLUMN',@level2name=N'BankCodes'
GO

CREATE TABLE [Payment].[PayType](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GWID] [bigint] NOT NULL,
	[Type] [tinyint] NOT NULL,
	[Currencies] [varchar](3) NOT NULL,
	[BankCode] [nvarchar](50) NOT NULL,
	[minAmount] [int] NOT NULL,
	[maxAmount] [int] NOT NULL,
	[amountFixed] [bit] NULL,
	[fixedAmount] [nvarchar](max) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastUpdateTime] [datetime] NULL,
 CONSTRAINT [PK_Payment_PayType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Payment].[PayType] ADD  DEFAULT ((0)) FOR [minAmount]
GO

ALTER TABLE [Payment].[PayType] ADD  DEFAULT ((0)) FOR [maxAmount]
GO

ALTER TABLE [Payment].[PayType] ADD  DEFAULT ((0)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Payment.PayGateway.ID' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayType', @level2type=N'COLUMN',@level2name=N'GWID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1:payin 2:payout' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayType', @level2type=N'COLUMN',@level2name=N'Type'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type為2時內容為NULL' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayType', @level2type=N'COLUMN',@level2name=N'amountFixed'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type為2時內容為NULL' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayType', @level2type=N'COLUMN',@level2name=N'fixedAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'狀態; 1:enable, 2:disable, 3:closed' , @level0type=N'SCHEMA',@level0name=N'Payment', @level1type=N'TABLE',@level1name=N'PayType', @level2type=N'COLUMN',@level2name=N'Status'
GO







