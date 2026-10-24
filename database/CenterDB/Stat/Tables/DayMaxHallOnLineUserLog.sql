CREATE TABLE [Stat].[DayMaxHallOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_DayMaxHallOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_DayMaxHallOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_DayMaxHallOnLineUserLog_StatTime]
    ON [Stat].[DayMaxHallOnLineUserLog]([StatTime] ASC);

