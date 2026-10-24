CREATE TABLE [Stat].[ServerOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [ServerID]     INT      NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_ServerOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_ServerOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ServerOnLineUserLog_ServerID_StatTime]
    ON [Stat].[ServerOnLineUserLog]([ServerID] ASC, [StatTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_ServerOnLineUserLog_StatTime_INCLUDE]
    ON [Stat].[ServerOnLineUserLog]([StatTime] ASC)
    INCLUDE([ServerID], [TotalCount], [PCCount], [IOSCount], [AndroidCount]);

