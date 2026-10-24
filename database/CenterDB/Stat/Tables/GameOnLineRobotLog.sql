CREATE TABLE [Stat].[GameOnLineRobotLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [GameID]     INT      NOT NULL,
    [TotalCount] INT      NOT NULL,
    [StatTime]   DATETIME NULL,
    CONSTRAINT [PK_RobotOnLineLog_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_RobotOnLineLog_StatTime]
    ON [Stat].[GameOnLineRobotLog]([StatTime] ASC);

