CREATE TABLE [Stat].[StatisticsGameUserAmount] (
    [StatTime]       DATETIME NOT NULL,
    [PlatformID]     INT      NOT NULL,
    [AreaID]         INT      NOT NULL,
    [GameID]         INT      NOT NULL,
    [UserID]         INT      NOT NULL,
    [TotalAmount]    BIGINT   CONSTRAINT [DF_StatisticsGameUserAmount_TotalAmount] DEFAULT ((0)) NOT NULL,
    [TotalTaxAmount] BIGINT   CONSTRAINT [DF_StatisticsGameUserAmount_TotalTaxAmount] DEFAULT ((0)) NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_StatisticsGameUserAmount_StatTime]
    ON [Stat].[StatisticsGameUserAmount]([StatTime] ASC);

