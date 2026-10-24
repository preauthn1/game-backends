CREATE TABLE [Area].[Platform] (
    [PlatformID]              INT           NOT NULL,
    [Name]                    VARCHAR (50)  NOT NULL,
    [UpGradeMsg]              VARCHAR (256) CONSTRAINT [DF_Platform_UpGradeMsg] DEFAULT ('') NOT NULL,
    [AnnMsg]                  VARCHAR (256) CONSTRAINT [DF_Platform_AnnMsg] DEFAULT ('') NOT NULL,
    [FAQContent]              VARCHAR (MAX) NOT NULL,
    [PaySort]                 VARCHAR (512) NOT NULL,
    [UIFlag]                  VARCHAR (32)  NOT NULL,
    [PayTip]                  VARCHAR (256) NOT NULL,
    [PayAmountConfig]         VARCHAR (MAX) NOT NULL,
    [PayStatus]               TINYINT       NOT NULL,
    [PayCloseTip]             VARCHAR (MAX) NOT NULL,
    [GiftMoney]               BIGINT        CONSTRAINT [DF_Platform_GiftMoney] DEFAULT ((0)) NOT NULL,
    [DefaultMoney]            BIGINT        CONSTRAINT [DF_Platform_DefaultMoney] DEFAULT ((0)) NOT NULL,
    [PlatformCreateTime]      DATETIME      CONSTRAINT [DF_Platform_PlatformCreateTime] DEFAULT (getdate()) NULL,
    [UISort]                  VARCHAR (32)  CONSTRAINT [DF_Platform_UISort] DEFAULT ('1,2,3,4') NOT NULL,
    [AnnMsgShow]              TINYINT       CONSTRAINT [DF_Platform_AnnMsgShow] DEFAULT ((0)) NOT NULL,
    [UseDefaultConfig]        BIT           CONSTRAINT [DF_Platform_UseDefaultConfig] DEFAULT ((1)) NOT NULL,
    [ShanFuAccountID]         VARCHAR (20)  NULL,
    [ShanFuSign]              VARCHAR (32)  NULL,
    [ShanFuPayAgentUrl]       VARCHAR (250) NULL,
    [ShanFuPayAgentClientUrl] VARCHAR (250) NULL,
    [Company]                 INT           NULL,
    CONSTRAINT [PK_Platform] PRIMARY KEY CLUSTERED ([PlatformID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'运营平台表', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'Platform';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'升级账号送金币', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'Platform', @level2type = N'COLUMN', @level2name = N'GiftMoney';

