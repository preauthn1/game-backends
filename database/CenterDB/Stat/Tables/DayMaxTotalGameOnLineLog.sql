CREATE TABLE [Stat].[DayMaxTotalGameOnLineLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_DayMaxTotalGameOnLineLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_DayMaxTotalGameOnLineLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_DayMaxTotalGameOnLineLog_StatTime]
    ON [Stat].[DayMaxTotalGameOnLineLog]([StatTime] ASC);

