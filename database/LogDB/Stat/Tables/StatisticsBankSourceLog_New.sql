CREATE TABLE [Stat].[StatisticsBankSourceLog_New] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT      NOT NULL,
    [AreaID]       INT      NOT NULL,
    [SourceTypeID] INT      NOT NULL,
    [Amount]       BIGINT   NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_StatisticsBankSourceLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

