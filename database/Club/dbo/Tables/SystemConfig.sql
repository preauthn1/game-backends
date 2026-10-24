CREATE TABLE [dbo].[SystemConfig] (
    [ID]              INT          NULL,
    [Class1ChargeFee] BIGINT       CONSTRAINT [DF_UpgradeConfig_Level1ChargeFee] DEFAULT ((0)) NOT NULL,
    [Class2ChargeFee] BIGINT       CONSTRAINT [DF_UpgradeConfig_Level2ChargeFee] DEFAULT ((0)) NOT NULL,
    [Class3ChargeFee] BIGINT       CONSTRAINT [DF_SystemConfig_Class2ChargeFee1] DEFAULT ((0)) NOT NULL,
    [Class4ChargeFee] BIGINT       CONSTRAINT [DF_SystemConfig_Class4ChargeFee] DEFAULT ((0)) NOT NULL,
    [OfficialUser1]   VARCHAR (50) NULL,
    [OfficialUser2]   VARCHAR (50) NULL,
    [OfficialUser3]   VARCHAR (50) NULL,
    [OfficialSite]    VARCHAR (50) NULL,
    [User1Type]       TINYINT      CONSTRAINT [DF_SystemConfig_User1Type_1] DEFAULT ((1)) NOT NULL,
    [User2Type]       TINYINT      CONSTRAINT [DF_SystemConfig_User2Type_1] DEFAULT ((1)) NOT NULL,
    [User3Type]       TINYINT      CONSTRAINT [DF_SystemConfig_User3Type_1] DEFAULT ((1)) NOT NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'成為一級代理最低要求充值金額', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'Class1ChargeFee';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'成為二級代理最低要求充值金額', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'Class2ChargeFee';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'成為二級代理最低要求充值金額', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'Class3ChargeFee';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'User1Type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'User2Type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemConfig', @level2type = N'COLUMN', @level2name = N'User3Type';

