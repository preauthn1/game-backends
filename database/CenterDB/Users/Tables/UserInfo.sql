CREATE TABLE [Users].[UserInfo] (
    [UserID]             INT          NOT NULL,
    [LoginName]          VARCHAR (32) NOT NULL,
    [Password]           VARCHAR (32) NOT NULL,
    [BankPassword]       VARCHAR (32) NOT NULL,
    [FaceID]             INT          NOT NULL,
    [NickName]           VARCHAR (32) NOT NULL,
    [Sex]                TINYINT      NOT NULL,
    [HDType]             TINYINT      NOT NULL,
    [HDCode]             VARCHAR (64) NOT NULL,
    [UserType]           TINYINT      NOT NULL,
    [CreateTime]         DATETIME     NOT NULL,
    [CreateIPAddress]    VARCHAR (15) NOT NULL,
    [LastLogonTime]      DATETIME     NOT NULL,
    [LastLogonIPAddress] VARCHAR (15) NOT NULL,
    [LastLogonHDType]    TINYINT      NULL,
    [LastLogonHDCode]    VARCHAR (64) NULL,
    [RealName]           VARCHAR (64) NULL,
    [AlipayAccount]      VARCHAR (64) NULL,
    [UnionpayBank]       VARCHAR (5)  NULL,
    [UnionpayAccount]    VARCHAR (64) NULL,
    [UnionpayRealName]   VARCHAR (64) NULL,
    [SumCharge]          BIGINT       CONSTRAINT [DF_UserInfo_SumCharge] DEFAULT ((0)) NOT NULL,
    [SumConvert]         BIGINT       CONSTRAINT [DF_UserInfo_SumConvert] DEFAULT ((0)) NOT NULL,
    [SumConvertTimes]    INT          CONSTRAINT [DF_UserInfo_SumConvertTimes] DEFAULT ((0)) NOT NULL,
    [SumTax]             BIGINT       CONSTRAINT [DF_UserInfo_SumTax] DEFAULT ((0)) NOT NULL,
    [SumLoginTimes]      INT          CONSTRAINT [DF_UserInfo_SumLoginTimes] DEFAULT ((0)) NOT NULL,
    [QuestionCount]      INT          CONSTRAINT [DF_UserInfo_QuestionTimes] DEFAULT ((0)) NOT NULL,
    [GradeID]            INT          CONSTRAINT [DF_UserInfo_GradeID] DEFAULT ((0)) NOT NULL,
    [AreaID]             INT          CONSTRAINT [DF_UserInfo_SiteID] DEFAULT ((1)) NOT NULL,
    [SumAgentCharge]     BIGINT       CONSTRAINT [DF_UserInfo_SumAgentCharge] DEFAULT ((0)) NOT NULL,
    [AdPlan]             INT          CONSTRAINT [DF_UserInfo_AdPlan] DEFAULT ((0)) NOT NULL,
    [AdPlanTime]         DATETIME     NULL,
    [ExperienceCash]     BIGINT       CONSTRAINT [DF_UserInfo_ExperienceCash_1] DEFAULT ((100000)) NULL,
    [SumConvertTax]      BIGINT       CONSTRAINT [DF_UserInfo_SumConvertTax] DEFAULT ((0)) NULL,
    [SumFreeMoney]       BIGINT       CONSTRAINT [DF_UserInfo_SumFreeMoney] DEFAULT ((0)) NULL,
    [SumClubMoney]       BIGINT       CONSTRAINT [DF_UserInfo_SumClubMoney] DEFAULT ((0)) NULL,
    [SumAdminMoney]      BIGINT       CONSTRAINT [DF_UserInfo_SumAdminMoney] DEFAULT ((0)) NULL,
    [SumFlowAmount]      BIGINT       DEFAULT ((0)) NOT NULL,
	[AvatarFrame]		INT	 NULL DEFAULT 1,
	[SumCouponAmount] BiGINT NULL DEFAULT 0
) ON [S_UserID] ([UserID]);


GO
CREATE CLUSTERED INDEX [IUX_UserInfo_UserID]
    ON [Users].[UserInfo]([UserID] ASC, [CreateTime] ASC, [LastLogonTime] ASC, [SumCharge] ASC, [SumConvert] ASC, [GradeID] ASC, [SumAgentCharge] ASC)
    ON [S_UserID] ([UserID]);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_UserInfo_LoginName_INCLUDE]
    ON [Users].[UserInfo]([LoginName] ASC)
    INCLUDE([UserID], [FaceID], [NickName], [Sex], [HDType], [HDCode], [UserType], [CreateTime], [CreateIPAddress], [LastLogonTime], [LastLogonIPAddress], [LastLogonHDType], [LastLogonHDCode], [RealName], [AlipayAccount], [UnionpayBank], [UnionpayAccount], [UnionpayRealName], [SumCharge], [SumConvert], [SumConvertTimes], [SumTax], [SumLoginTimes], [QuestionCount], [GradeID], [AreaID], [SumAgentCharge])
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserInfo_APA_UPA_INCLUDE]
    ON [Users].[UserInfo]([AlipayAccount] ASC, [UnionpayAccount] ASC)
    INCLUDE([UserID], [LoginName], [FaceID], [Sex], [HDType], [HDCode], [UserType], [CreateTime], [CreateIPAddress], [LastLogonTime], [LastLogonIPAddress], [LastLogonHDType], [LastLogonHDCode], [RealName], [SumCharge], [SumConvert], [SumConvertTimes], [SumTax], [SumLoginTimes], [GradeID], [AreaID], [SumAgentCharge])
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserInfo_HDT_HDC_INCLUDE]
    ON [Users].[UserInfo]([UserType] ASC, [HDCode] ASC, [HDType] ASC)
    INCLUDE([FaceID], [Sex], [UserID], [CreateIPAddress], [LastLogonIPAddress], [AreaID], [LoginName], [NickName])
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserInfo_UID_UT_AID_INCLUDE]
    ON [Users].[UserInfo]([UserID] ASC, [UserType] ASC, [AreaID] ASC)
    INCLUDE([FaceID], [Sex], [HDType], [HDCode], [CreateTime], [CreateIPAddress], [LastLogonTime], [LastLogonIPAddress], [LastLogonHDType], [LastLogonHDCode], [RealName], [AlipayAccount], [UnionpayAccount], [SumCharge], [SumConvert], [SumConvertTimes], [SumTax], [SumLoginTimes], [SumAgentCharge], [LoginName])
    ON [S_UserID] ([UserID]);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 普通账号 2 机器人账号 3 快速登录账号', @level0type = N'SCHEMA', @level0name = N'Users', @level1type = N'TABLE', @level1name = N'UserInfo', @level2type = N'COLUMN', @level2name = N'UserType';

