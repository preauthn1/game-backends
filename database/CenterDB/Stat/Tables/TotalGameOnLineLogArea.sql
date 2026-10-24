CREATE TABLE [Stat].[TotalGameOnLineLogArea] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [AreaID]       INT      NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_TotalGameOnLineLogArea_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_TotalGameOnLineLogArea_StatTime]
    ON [Stat].[TotalGameOnLineLogArea]([StatTime] ASC);

