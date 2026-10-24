CREATE TABLE [dbo].[OfficialUserConfig] (
    [PlatformID]    INT          NOT NULL,
    [OfficialUser1] VARCHAR (50) NULL,
    [OfficialUser2] VARCHAR (50) NULL,
    [OfficialUser3] VARCHAR (50) NULL,
    [User1Type]     TINYINT      CONSTRAINT [DF_OfficialUserConfig_User1Type] DEFAULT ((1)) NOT NULL,
    [User2Type]     TINYINT      CONSTRAINT [DF_OfficialUserConfig_User2Type] DEFAULT ((1)) NOT NULL,
    [User3Type]     TINYINT      CONSTRAINT [DF_OfficialUserConfig_User3Type] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_OfficialUserConfig] PRIMARY KEY CLUSTERED ([PlatformID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'OfficialUserConfig', @level2type = N'COLUMN', @level2name = N'User1Type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'OfficialUserConfig', @level2type = N'COLUMN', @level2name = N'User2Type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 weixin 2 QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'OfficialUserConfig', @level2type = N'COLUMN', @level2name = N'User3Type';

