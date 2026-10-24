CREATE TABLE [Stat].[TotalGameOnLineRobotLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [TotalCount] INT      NOT NULL,
    [StatTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_TotalGameOnLineRobotLog_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_TotalGameOnLineRobotLog_StatTime]
    ON [Stat].[TotalGameOnLineRobotLog]([StatTime] ASC);

