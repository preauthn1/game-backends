CREATE TABLE [Stat].[ServerOnLineRobotLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [ServerID]   INT      NOT NULL,
    [TotalCount] INT      NOT NULL,
    [StatTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_ServerOnLineRobotLog_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_ServerOnLineRobotLog_StatTime]
    ON [Stat].[ServerOnLineRobotLog]([StatTime] ASC);

