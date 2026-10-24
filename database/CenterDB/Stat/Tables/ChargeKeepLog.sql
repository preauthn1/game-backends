CREATE TABLE [Stat].[ChargeKeepLog] (
    [Id]             INT        IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]       INT        NOT NULL,
    [RegChargeUsers] INT        CONSTRAINT [DF_ChargeKeepLog_RegUsers] DEFAULT ((0)) NULL,
    [HDType]         INT        NULL,
    [Day1Keep]       FLOAT (53) CONSTRAINT [DF_ChargeKeepLog_1DayKeep] DEFAULT ((0)) NULL,
    [Day2Keep]       FLOAT (53) CONSTRAINT [DF_ChargeKeepLog_2DayKeep] DEFAULT ((0)) NULL,
    [Day7Keep]       FLOAT (53) CONSTRAINT [DF_ChargeKeepLog_7DayKeep] DEFAULT ((0)) NULL,
    [Day30Keep]      FLOAT (53) CONSTRAINT [DF_ChargeKeepLog_30DayKeep] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_ChargeKeepLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

