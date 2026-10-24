CREATE TABLE [Stat].[DayMaxServerOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_DayMaxServerOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [ServerID]     INT      NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_DayMaxServerOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_DayMaxServerOnLineUserLog_ServerID_StatTime]
    ON [Stat].[DayMaxServerOnLineUserLog]([ServerID] ASC, [StatTime] ASC);

