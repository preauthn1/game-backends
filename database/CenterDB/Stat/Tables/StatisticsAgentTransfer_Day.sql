CREATE TABLE [Stat].[StatisticsAgentTransfer_Day] (
    [LogID]        BIGINT   IDENTITY (1, 1) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    [PlatformID]   INT      NULL,
    [AreaID]       INT      NULL,
    [AgentID]      INT      NULL,
    [SourceID]     INT      NULL,
    [Amount]       BIGINT   NULL,
    [TargetCounts] INT      NULL,
    CONSTRAINT [PK_StatisticsAgentTransfer_Day] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

