CREATE TABLE [Stat].[StatisticsUserAmount_Day] (
    [LogDate]          DATETIME NOT NULL,
    [UserID]           INT      NOT NULL,
    [PlatformID]       INT      NOT NULL,
    [AreaID]           INT      NOT NULL,
    [ChargeAmount]     BIGINT   CONSTRAINT [DF_StatisticsUserAmount_Day_ChargeAmount] DEFAULT ((0)) NULL,
    [ConvertAmount]    BIGINT   CONSTRAINT [DF_StatisticsUserAmount_Day_ConvertAmount] DEFAULT ((0)) NULL,
    [ConvertTaxAmount] BIGINT   NULL,
    [FlowAmount]       BIGINT   CONSTRAINT [DF_StatisticsUserAmount_Day_FlowAmount] DEFAULT ((0)) NULL,
    [GameAmount]       BIGINT   NULL,
    [GameTaxAmount]    BIGINT   CONSTRAINT [DF_StatisticsUserAmount_Day_TaxAmount] DEFAULT ((0)) NULL,
    [ChargeCount]      INT      CONSTRAINT [DF_StatisticsUserAmount_Day_ChargeCount] DEFAULT ((0)) NULL,
    [ConvertCount]     INT      CONSTRAINT [DF_StatisticsUserAmount_Day_ConvertCount] DEFAULT ((0)) NULL,
    [ConvertTaxCount]  INT      NULL,
    [FlowCount]        INT      CONSTRAINT [DF_StatisticsUserAmount_Day_FlowCount] DEFAULT ((0)) NULL,
    [GameAmountCount]  INT      NULL,
    [GameTaxCount]     INT      CONSTRAINT [DF_StatisticsUserAmount_Day_TaxCount] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_StatisticsUserAmount_Day] PRIMARY KEY CLUSTERED ([LogDate] ASC, [UserID] ASC)
);

