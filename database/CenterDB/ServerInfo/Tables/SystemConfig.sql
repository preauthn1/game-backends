CREATE TABLE [ServerInfo].[SystemConfig] (
    [Id]                     INT           IDENTITY (1, 1) NOT NULL,
    [ConvertStatus]          TINYINT       NOT NULL,
    [PromptMsg]              VARCHAR (256) NOT NULL,
    [AutoCheckLimit]         BIGINT        NOT NULL,
    [ConvertRetainAmount]    BIGINT        CONSTRAINT [DF_ConvertConfig_ConvertRetainAmount] DEFAULT ((6)) NOT NULL,
    [ConvertMinAmount]       BIGINT        CONSTRAINT [DF_ConvertConfig_ConvertMinAmount] DEFAULT ((5000)) NOT NULL,
    [ConvertMaxAmount]       BIGINT        CONSTRAINT [DF_ConvertConfig_ConvertMaxAmount] DEFAULT ((50000)) NOT NULL,
    [ConvertRateTip]         VARCHAR (256) CONSTRAINT [DF_ConvertConfig_ConvertRateTip] DEFAULT ('兑换比例xxx') NOT NULL,
    [BankDefaultPwdTip]      VARCHAR (256) CONSTRAINT [DF_ConvertConfig_BankDefaultPwdTip] DEFAULT ('银行默认密码888888') NOT NULL,
    [QuestionContactTip]     VARCHAR (256) CONSTRAINT [DF_SystemConfig_QuestionContactTip] DEFAULT ('') NOT NULL,
    [ConvertBankShow]        TINYINT       CONSTRAINT [DF_SystemConfig_ConvertBankShow] DEFAULT ((1)) NOT NULL,
    [ConvertAlipayShow]      TINYINT       CONSTRAINT [DF_SystemConfig_ConvertAlipayShow] DEFAULT ((1)) NOT NULL,
    [ConvertWinLostAlert]    BIGINT        CONSTRAINT [DF_SystemConfig_ConverWinLostAlert_1] DEFAULT ((0)) NULL,
    [TGBotToken]             VARCHAR (100) NULL,
    [TGChatID]               VARCHAR (200) NULL,
    [SystemTag]              VARCHAR (20)  CONSTRAINT [DF_SystemConfig_SystemTag] DEFAULT ('--') NULL,
    [ConvertAlipayMinAmount] BIGINT        NOT NULL,
    [ConvertAlipayMaxAmount] BIGINT        NOT NULL
);




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'兑换配置信息', @level0type = N'SCHEMA', @level0name = N'ServerInfo', @level1type = N'TABLE', @level1name = N'SystemConfig';

