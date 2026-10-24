CREATE TABLE [Stat].[HallOnLineUserLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_HallOnLineUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_HallOnLineUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_HallOnLineUserLog_StatTime]
    ON [Stat].[HallOnLineUserLog]([StatTime] ASC);

