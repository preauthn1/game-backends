CREATE TABLE [Stat].[AreaDataDay] (
    [Id]                   INT           IDENTITY (1, 1) NOT NULL,
    [StatTime]             DATETIME      NOT NULL,
    [PlatformID]           INT           CONSTRAINT [DF_AreaDataDay_PlatformID] DEFAULT ((1)) NOT NULL,
    [AreaID]               INT           NOT NULL,
    [GameID]               INT           NULL,
    [ServerID]             INT           NULL,
    [OldLogin]             BIGINT        CONSTRAINT [DF_AreaDataDay_OldLogin] DEFAULT ((0)) NOT NULL,
    [NewLogin]             BIGINT        CONSTRAINT [DF_AreaDataDay_NewLogin] DEFAULT ((0)) NOT NULL,
    [OldCharge]            BIGINT        CONSTRAINT [DF_AreaDataDay_OldCharge] DEFAULT ((0)) NOT NULL,
    [NewCharge]            BIGINT        CONSTRAINT [DF_AreaDataDay_NewCharge] DEFAULT ((0)) NOT NULL,
    [OldConvert]           BIGINT        CONSTRAINT [DF_AreaDataDay_OldConvert] DEFAULT ((0)) NOT NULL,
    [NewConvert]           BIGINT        CONSTRAINT [DF_AreaDataDay_NewConvert] DEFAULT ((0)) NOT NULL,
    [OldConvertTax]        BIGINT        CONSTRAINT [DF_AreaDataDay_OldConvertTax] DEFAULT ((0)) NOT NULL,
    [NewConvertTax]        BIGINT        CONSTRAINT [DF_AreaDataDay_NewConvertTax] DEFAULT ((0)) NOT NULL,
    [LoginIP]              BIGINT        CONSTRAINT [DF_AreaDataDay_LoginIP] DEFAULT ((0)) NOT NULL,
    [OldLoginIP]           BIGINT        CONSTRAINT [DF_AreaDataDay_OldLoginIP] DEFAULT ((0)) NOT NULL,
    [NewLoginIP]           BIGINT        CONSTRAINT [DF_AreaDataDay_NewLoginIP] DEFAULT ((0)) NOT NULL,
    [LoginMac]             BIGINT        CONSTRAINT [DF_AreaDataDay_LoginMac] DEFAULT ((0)) NOT NULL,
    [OldLoginMac]          BIGINT        CONSTRAINT [DF_AreaDataDay_OldLoginMac] DEFAULT ((0)) NOT NULL,
    [NewLoginMac]          BIGINT        CONSTRAINT [DF_AreaDataDay_NewLoginMac] DEFAULT ((0)) NOT NULL,
    [GameTax]              BIGINT        CONSTRAINT [DF_AreaDataDay_GameTax] DEFAULT ((0)) NOT NULL,
    [GameTaxDetail]        VARCHAR (MAX) CONSTRAINT [DF_AreaDataDay_GameTaxDetail] DEFAULT ('') NOT NULL,
    [Total_OrderUser]      BIGINT        CONSTRAINT [DF_AreaDataDay_OldPayUser] DEFAULT ((0)) NULL,
    [NewOrderUser]         BIGINT        CONSTRAINT [DF_AreaDataDay_NewPayUser] DEFAULT ((0)) NULL,
    [iOSCount]             INT           CONSTRAINT [DF_AreaDataDay_iOSCount] DEFAULT ((0)) NULL,
    [NewiOSCount]          INT           CONSTRAINT [DF_AreaDataDay_NewiOSCount] DEFAULT ((0)) NULL,
    [AndRoidCount]         INT           CONSTRAINT [DF_AreaDataDay_AndRoidCount] DEFAULT ((0)) NULL,
    [NewAndRoidCount]      INT           CONSTRAINT [DF_AreaDataDay_NewAndRoidCount] DEFAULT ((0)) NULL,
    [PCCount]              INT           CONSTRAINT [DF_AreaDataDay_PCCount] DEFAULT ((0)) NULL,
    [NewPCCount]           INT           CONSTRAINT [DF_AreaDataDay_NewPCCount] DEFAULT ((0)) NULL,
    [AmountTotal]          BIGINT        CONSTRAINT [DF_AreaDataDay_AmountTotal] DEFAULT ((0)) NULL,
    [NewAmountTotal]       BIGINT        CONSTRAINT [DF_AreaDataDay_NewAmountTotal] DEFAULT ((0)) NULL,
    [AmountAvg]            BIGINT        CONSTRAINT [DF_AreaDataDay_AmountAvg] DEFAULT ((0)) NULL,
    [NewAmountAvg]         BIGINT        CONSTRAINT [DF_AreaDataDay_NewAmountAvg] DEFAULT ((0)) NULL,
    [TaxAmountTotal]       BIGINT        CONSTRAINT [DF_AreaDataDay_TaxAmountTotal] DEFAULT ((0)) NULL,
    [NewTaxAmountTotal]    BIGINT        CONSTRAINT [DF_AreaDataDay_NewTaxAmountTotal] DEFAULT ((0)) NULL,
    [TaxAmountAvg]         BIGINT        CONSTRAINT [DF_AreaDataDay_TaxAmountAvg] DEFAULT ((0)) NULL,
    [NewTaxAmountAvg]      BIGINT        CONSTRAINT [DF_AreaDataDay_NewTaxAmountAvg] DEFAULT ((0)) NULL,
    [GameTimeMax]          INT           CONSTRAINT [DF_AreaDataDay_GameTimeMax] DEFAULT ((0)) NULL,
    [GameTimeMin]          INT           CONSTRAINT [DF_AreaDataDay_GameTimeMin] DEFAULT ((0)) NULL,
    [GameTimeAvg]          INT           CONSTRAINT [DF_AreaDataDay_GameTimeAvg] DEFAULT ((0)) NULL,
    [RobotTaxAmount]       BIGINT        CONSTRAINT [DF__AreaDataD__Robot__4F32B74A] DEFAULT ((0)) NOT NULL,
    [TotalConvertUser]     INT           CONSTRAINT [DF__AreaDataD__Total__7FD5EEA5] DEFAULT ((0)) NOT NULL,
    [NewConvertUser]       INT           CONSTRAINT [DF_AreaDataDay_NewConvertUser] DEFAULT ((0)) NULL,
    [TotalConvertCounts]   INT           CONSTRAINT [DF__AreaDataD__Total__00CA12DE] DEFAULT ((0)) NOT NULL,
    [TotalOrderCounts]     INT           CONSTRAINT [DF__AreaDataD__Total__01BE3717] DEFAULT ((0)) NOT NULL,
    [NewOrderCounts]       INT           CONSTRAINT [DF__AreaDataD__NewOr__02B25B50] DEFAULT ((0)) NOT NULL,
    [NewAgentChargeUser]   INT           CONSTRAINT [DF_AreaDataDay_NewAgentChargeUser] DEFAULT ((0)) NULL,
    [OldAgentChargeUser]   INT           CONSTRAINT [DF_AreaDataDay_OldAgentChargeUser] DEFAULT ((0)) NULL,
    [NewAgentChargeAmount] BIGINT        CONSTRAINT [DF_AreaDataDay_NewAgentChargeAmount] DEFAULT ((0)) NULL,
    [OldAgentChargeAmount] BIGINT        CONSTRAINT [DF_AreaDataDay_OldAgentChargeAmount] DEFAULT ((0)) NULL,
    [TotalMoney]           BIGINT        CONSTRAINT [DF_AreaDataDay_TotalMoney] DEFAULT ((0)) NULL,
    [TotalBankMoney]       BIGINT        CONSTRAINT [DF_AreaDataDay_TotalBankMoney] DEFAULT ((0)) NULL,
    [UserGameAmount]       BIGINT        CONSTRAINT [DF_AreaDataDay_UserGameAmount] DEFAULT ((0)) NULL,
    [RobotGameAmount]      BIGINT        CONSTRAINT [DF_AreaDataDay_RobotGameAmount] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_DayAreaDataStat_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AreaDataStatDay_StatTime]
    ON [Stat].[AreaDataDay]([StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_DayAreaDataStat_AreaID]
    ON [Stat].[AreaDataDay]([AreaID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'渠道日报表统计数据', @level0type = N'SCHEMA', @level0name = N'Stat', @level1type = N'TABLE', @level1name = N'AreaDataDay';

