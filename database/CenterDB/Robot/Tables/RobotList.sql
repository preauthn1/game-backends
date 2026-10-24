CREATE TABLE [Robot].[RobotList] (
    [UserID]   INT      NOT NULL,
    [ServerID] INT      NOT NULL,
    [LevelID]  INT      NOT NULL,
    [Group]    CHAR (1) CONSTRAINT [DF_RobotList_Group] DEFAULT ('') NOT NULL,
    [Sort]     INT      CONSTRAINT [DF_RobotList_Sort] DEFAULT (abs(checksum(newid()))) NOT NULL,
    CONSTRAINT [PK_RobotList] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_RobotList_ServerID_LevelID]
    ON [Robot].[RobotList]([ServerID] ASC, [LevelID] ASC)
    INCLUDE([UserID], [Sort]);

