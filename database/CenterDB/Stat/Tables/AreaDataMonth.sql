CREATE TABLE [Stat].[AreaDataMonth] (
    [Id]                   INT           IDENTITY (1, 1) NOT NULL,
    [StatTime]             DATETIME      NOT NULL,
    [PlatformID]           INT           NOT NULL,
    [AreaID]               INT           NOT NULL,
    [GameID]               INT           NULL,
    [ServerID]             INT           NULL,
    [OldLogin]             BIGINT        CONSTRAINT [DF_AreaDataMonth_OldLogin] DEFAULT ((0)) NOT NULL,
    [NewLogin]             BIGINT        CONSTRAINT [DF_AreaDataMonth_NewLogin] DEFAULT ((0)) NOT NULL,
    [OldCharge]            BIGINT        CONSTRAINT [DF_AreaDataMonth_OldCharge] DEFAULT ((0)) NOT NULL,
    [NewCharge]            BIGINT        CONSTRAINT [DF_AreaDataMonth_NewCharge] DEFAULT ((0)) NOT NULL,
    [OldConvert]           BIGINT        CONSTRAINT [DF_AreaDataMonth_OldConvert] DEFAULT ((0)) NOT NULL,
    [NewConvert]           BIGINT        CONSTRAINT [DF_AreaDataMonth_NewConvert] DEFAULT ((0)) NOT NULL,
    [OldConvertTax]        BIGINT        CONSTRAINT [DF_AreaDataMonth_OldConvertTax] DEFAULT ((0)) NOT NULL,
    [NewConvertTax]        BIGINT        CONSTRAINT [DF_AreaDataMonth_NewConvertTax] DEFAULT ((0)) NOT NULL,
    [LoginIP]              BIGINT        CONSTRAINT [DF_AreaDataMonth_LoginIP] DEFAULT ((0)) NOT NULL,
    [OldLoginIP]           BIGINT        CONSTRAINT [DF_AreaDataMonth_OldLoginIP] DEFAULT ((0)) NOT NULL,
    [NewLoginIP]           BIGINT        CONSTRAINT [DF_AreaDataMonth_NewLoginIP] DEFAULT ((0)) NOT NULL,
    [LoginMac]             BIGINT        CONSTRAINT [DF_AreaDataMonth_LoginMac] DEFAULT ((0)) NOT NULL,
    [OldLoginMac]          BIGINT        CONSTRAINT [DF_AreaDataMonth_OldLoginMac] DEFAULT ((0)) NOT NULL,
    [NewLoginMac]          BIGINT        CONSTRAINT [DF_AreaDataMonth_NewLoginMac] DEFAULT ((0)) NOT NULL,
    [GameTax]              BIGINT        CONSTRAINT [DF_AreaDataMonth_GameTax] DEFAULT ((0)) NOT NULL,
    [GameTaxDetail]        VARCHAR (MAX) CONSTRAINT [DF_AreaDataMonth_GameTaxDetail] DEFAULT ('') NOT NULL,
    [Total_OrderUser]      BIGINT        CONSTRAINT [DF_AreaDataMonth_Total_OrderUser] DEFAULT ((0)) NULL,
    [NewOrderUser]         BIGINT        CONSTRAINT [DF_AreaDataMonth_NewOrderUser] DEFAULT ((0)) NULL,
    [iOSCount]             INT           CONSTRAINT [DF_AreaDataMonth_iOSCount] DEFAULT ((0)) NULL,
    [NewiOSCount]          INT           CONSTRAINT [DF_AreaDataMonth_NewiOSCount] DEFAULT ((0)) NULL,
    [AndRoidCount]         INT           CONSTRAINT [DF_AreaDataMonth_AndRoidCount] DEFAULT ((0)) NULL,
    [NewAndRoidCount]      INT           CONSTRAINT [DF_AreaDataMonth_NewAndRoidCount] DEFAULT ((0)) NULL,
    [PCCount]              INT           CONSTRAINT [DF_AreaDataMonth_PCCount] DEFAULT ((0)) NULL,
    [NewPCCount]           INT           CONSTRAINT [DF_AreaDataMonth_NewPCCount] DEFAULT ((0)) NULL,
    [AmountTotal]          BIGINT        CONSTRAINT [DF_AreaDataMonth_AmountTotal] DEFAULT ((0)) NULL,
    [NewAmountTotal]       BIGINT        CONSTRAINT [DF_AreaDataMonth_NewAmountTotal] DEFAULT ((0)) NULL,
    [AmountAvg]            BIGINT        CONSTRAINT [DF_AreaDataMonth_AmountAvg] DEFAULT ((0)) NULL,
    [NewAmountAvg]         BIGINT        CONSTRAINT [DF_AreaDataMonth_NewAmountAvg] DEFAULT ((0)) NULL,
    [TaxAmountTotal]       BIGINT        CONSTRAINT [DF_AreaDataMonth_TaxAmountTotal] DEFAULT ((0)) NULL,
    [NewTaxAmountTotal]    BIGINT        CONSTRAINT [DF_AreaDataMonth_NewTaxAmountTotal] DEFAULT ((0)) NULL,
    [TaxAmountAvg]         BIGINT        CONSTRAINT [DF_AreaDataMonth_TaxAmountAvg] DEFAULT ((0)) NULL,
    [NewTaxAmountAvg]      BIGINT        CONSTRAINT [DF_AreaDataMonth_NewTaxAmountAvg] DEFAULT ((0)) NULL,
    [GameTimeMax]          INT           CONSTRAINT [DF_AreaDataMonth_GameTimeMax] DEFAULT ((0)) NULL,
    [GameTimeMin]          INT           CONSTRAINT [DF_AreaDataMonth_GameTimeMin] DEFAULT ((0)) NULL,
    [GameTimeAvg]          INT           CONSTRAINT [DF_AreaDataMonth_GameTimeAvg] DEFAULT ((0)) NULL,
    [RobotTaxAmount]       BIGINT        DEFAULT ((0)) NOT NULL,
    [TotalConvertUser]     INT           DEFAULT ((0)) NOT NULL,
    [TotalConvertCounts]   INT           DEFAULT ((0)) NOT NULL,
    [TotalOrderCounts]     INT           DEFAULT ((0)) NOT NULL,
    [NewOrderCounts]       INT           DEFAULT ((0)) NOT NULL,
    [NewAgentChargeUser]   INT           CONSTRAINT [DF_AreaDataMonth_NewAgentChargeUser] DEFAULT ((0)) NULL,
    [OldAgentChargeUser]   INT           CONSTRAINT [DF_AreaDataMonth_OldAgentChargeUser] DEFAULT ((0)) NULL,
    [NewAgentChargeAmount] BIGINT        CONSTRAINT [DF_AreaDataMonth_NewAgentChargeAmount] DEFAULT ((0)) NULL,
    [OldAgentChargeAmount] BIGINT        CONSTRAINT [DF_AreaDataMonth_OldAgentChargeAmount] DEFAULT ((0)) NULL,
    [NewConvertUser]       INT           CONSTRAINT [DF_AreaDataMonth_NewConvertUser] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_AreaDataMonth] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AreaDataMonth_StatTime]
    ON [Stat].[AreaDataMonth]([StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AreaDataMonth_AreaID]
    ON [Stat].[AreaDataMonth]([AreaID] ASC);

