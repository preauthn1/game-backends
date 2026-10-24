CREATE TABLE [Stat].[StatisticsDAU] (
    [StatTime]     DATETIME NOT NULL,
    [PlatformID]   INT      NOT NULL,
    [AreaID]       INT      NOT NULL,
    [GameID]       INT      NOT NULL,
    [PCCount]      INT      NULL,
    [AndroidCount] INT      NULL,
    [IOSCount]     INT      NULL,
    [TotalCount]   INT      NULL,
    [TotalTimes]   INT      CONSTRAINT [DF_StatisticsDAU_TotalTimes] DEFAULT ((0)) NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_StatisticsDAU_StatTime]
    ON [Stat].[StatisticsDAU]([StatTime] ASC);

