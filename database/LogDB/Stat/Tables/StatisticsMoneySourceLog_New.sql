CREATE TABLE [Stat].[StatisticsMoneySourceLog_New] (
    [ID]             INT      IDENTITY (1, 1) NOT NULL,
    [StatTime]       DATETIME NOT NULL,
    [PlatformID]     INT      NOT NULL,
    [AreaID]         INT      NOT NULL,
    [SourceTypeID]   INT      NOT NULL,
    [Amount]         BIGINT   NOT NULL,
    [RobotAmount]    BIGINT   NOT NULL,
    [RobotTaxAmount] BIGINT   NOT NULL,
    [TaxAmount]      BIGINT   NOT NULL,
    [FlowAmount]     BIGINT   NULL,
    CONSTRAINT [PK_StatisticsMoneySourceLog_New] PRIMARY KEY CLUSTERED ([ID] ASC)
);

