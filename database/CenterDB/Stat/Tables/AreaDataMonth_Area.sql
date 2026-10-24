CREATE TABLE [Stat].[AreaDataMonth_Area] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [StatTime]           DATETIME      NOT NULL,
    [PlatformID]         INT           NOT NULL,
    [AreaID]             INT           NOT NULL,
    [GameID]             INT           NULL,
    [ServerID]           INT           NULL,
    [OldLogin]           BIGINT        NOT NULL,
    [NewLogin]           BIGINT        NOT NULL,
    [OldCharge]          BIGINT        NOT NULL,
    [NewCharge]          BIGINT        NOT NULL,
    [OldConvert]         BIGINT        NOT NULL,
    [NewConvert]         BIGINT        NOT NULL,
    [OldConvertTax]      BIGINT        NOT NULL,
    [NewConvertTax]      BIGINT        NOT NULL,
    [LoginIP]            BIGINT        NOT NULL,
    [OldLoginIP]         BIGINT        NOT NULL,
    [NewLoginIP]         BIGINT        NOT NULL,
    [LoginMac]           BIGINT        NOT NULL,
    [OldLoginMac]        BIGINT        NOT NULL,
    [NewLoginMac]        BIGINT        NOT NULL,
    [GameTax]            BIGINT        NOT NULL,
    [GameTaxDetail]      VARCHAR (MAX) NOT NULL,
    [Total_OrderUser]    BIGINT        NULL,
    [NewOrderUser]       BIGINT        NULL,
    [iOSCount]           INT           NULL,
    [NewiOSCount]        INT           NULL,
    [AndRoidCount]       INT           NULL,
    [NewAndRoidCount]    INT           NULL,
    [PCCount]            INT           NULL,
    [NewPCCount]         INT           NULL,
    [AmountTotal]        BIGINT        NULL,
    [NewAmountTotal]     BIGINT        NULL,
    [AmountAvg]          BIGINT        NULL,
    [NewAmountAvg]       BIGINT        NULL,
    [TaxAmountTotal]     BIGINT        NULL,
    [NewTaxAmountTotal]  BIGINT        NULL,
    [TaxAmountAvg]       BIGINT        NULL,
    [NewTaxAmountAvg]    BIGINT        NULL,
    [GameTimeMax]        INT           NULL,
    [GameTimeMin]        INT           NULL,
    [GameTimeAvg]        INT           NULL,
    [RobotTaxAmount]     BIGINT        DEFAULT ((0)) NOT NULL,
    [TotalConvertUser]   INT           DEFAULT ((0)) NOT NULL,
    [TotalConvertCounts] INT           DEFAULT ((0)) NOT NULL,
    [TotalOrderCounts]   INT           DEFAULT ((0)) NOT NULL,
    [NewOrderCounts]     INT           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AreaDataMonth_Area] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AreaDataMonth_Area_StatTime]
    ON [Stat].[AreaDataMonth_Area]([StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AreaDataMonth_Area_AreaID]
    ON [Stat].[AreaDataMonth_Area]([AreaID] ASC);

