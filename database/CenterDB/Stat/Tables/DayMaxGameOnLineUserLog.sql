CREATE TABLE [Stat].[DayMaxGameOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [GameID]       INT      NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_DayMaxGameOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_DayMaxGameOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_DayMaxGameOnLineUserLog_GameID_StatTime]
    ON [Stat].[DayMaxGameOnLineUserLog]([GameID] ASC, [StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_DayMaxGameOnLineUserLog_PID_ST_INCLUDE]
    ON [Stat].[DayMaxGameOnLineUserLog]([PlatformID] ASC, [StatTime] ASC)
    INCLUDE([GameID], [TotalCount]);

