CREATE TABLE [Stat].[DAULog] (
    [StatTime]     DATETIME NOT NULL,
    [PlatformID]   INT      NOT NULL,
    [AreaID]       INT      NOT NULL,
    [GameID]       INT      NOT NULL,
    [PCCount]      INT      NULL,
    [AndroidCount] INT      NULL,
    [IOSCount]     INT      NULL,
    [TotalCount]   INT      NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_DAULog_StatTime]
    ON [Stat].[DAULog]([StatTime] ASC);

