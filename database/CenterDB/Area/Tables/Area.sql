CREATE TABLE [Area].[Area] (
    [AreaID]                INT            NOT NULL,
    [AreaName]              VARCHAR (32)   NOT NULL,
    [ClientPlatform]        VARCHAR (32)   NOT NULL,
    [PlatformID]            INT            NOT NULL,
    [Memo]                  VARCHAR (256)  NOT NULL,
    [AdminID]               INT            NOT NULL,
    [Sort]                  INT            NOT NULL,
    [ShareURL]              VARCHAR (500)  NOT NULL,
    [ShowConvertLimit]      BIGINT         NOT NULL,
    [ShowAllGameLimit]      BIGINT         NOT NULL,
    [ShowVIPAgent]          BIGINT         NOT NULL,
    [ShowCS]                BIGINT         NOT NULL,
    [ShowConvertBindPhone]  TINYINT        NOT NULL,
    [ShowAllGameBindPhone]  TINYINT        NOT NULL,
    [ShowVIPAgentBindPhone] TINYINT        NOT NULL,
    [ShowCSBindPhone]       TINYINT        NOT NULL,
    [PlaytimeCS]            INT            NOT NULL,
    [PlaytimeConvert]       INT            NOT NULL,
    [PlaytimeVIPAgent]      INT            NOT NULL,
    [CreateTime]            DATETIME       NOT NULL,
    [AreaStatus]            BIT            NOT NULL,
    [FAQContent]            VARCHAR (3000) NULL,
    [PayTips]               VARCHAR (256)  NULL,
    [UpGradeMsg]            VARCHAR (256)  NULL,
    [AnnMsg]                VARCHAR (256)  NULL,
    [AnnMsgShow]            TINYINT        NULL,
    [SetDefault]            TINYINT        NULL,
    [AreaConfig]            VARCHAR (5000) NULL,
    [GiftMoney]             BIGINT         NOT NULL,
    [DefaultMoney]          BIGINT         NOT NULL,
    [ShopNameID]            TINYINT        NOT NULL,
    [AgentNameID]           TINYINT        NOT NULL,
    [ShowConvertPopup]      TINYINT        CONSTRAINT [DF_Area_ShowConvertPopup] DEFAULT ((0)) NOT NULL,
    [ConvertPopupText]      VARCHAR (256)  CONSTRAINT [DF_Area_ConvertPopupText] DEFAULT ('') NULL, 
    [ShowCampaignAtLogin] TINYINT NOT NULL DEFAULT 0, 
    [ShowFirstDepositAtLogin] TINYINT NOT NULL DEFAULT 0
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'兑换未达成强制显示', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'Area', @level2type = N'COLUMN', @level2name = N'ShowConvertPopup';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'兑换禁止提示文本', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'Area', @level2type = N'COLUMN', @level2name = N'ConvertPopupText';

