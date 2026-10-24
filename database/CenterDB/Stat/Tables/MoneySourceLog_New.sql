CREATE TABLE [Stat].[MoneySourceLog_New] (
    [Id]             INT      IDENTITY (1, 1) NOT NULL,
    [StatTime]       DATETIME NOT NULL,
    [SourceID]       INT      NOT NULL,
    [PlatformID]     INT      NOT NULL,
    [AreaID]         INT      NOT NULL,
    [Amount]         BIGINT   NOT NULL,
    [RobotAmount]    BIGINT   NOT NULL,
    [RobotTaxAmount] BIGINT   NOT NULL,
    [TaxAmount]      BIGINT   NOT NULL,
    [FlowAmount]     BIGINT   NULL,
    CONSTRAINT [PK_MoneySourceLog_New] PRIMARY KEY CLUSTERED ([Id] ASC)
);

