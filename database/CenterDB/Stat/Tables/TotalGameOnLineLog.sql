CREATE TABLE [Stat].[TotalGameOnLineLog] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT      CONSTRAINT [DF_TotalGameOnLineLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [TotalCount]   INT      NOT NULL,
    [PCCount]      INT      NOT NULL,
    [IOSCount]     INT      NOT NULL,
    [AndroidCount] INT      NOT NULL,
    [StatTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_TotalGameOnLineLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_TotalGameOnLineLog_StatTime]
    ON [Stat].[TotalGameOnLineLog]([StatTime] ASC);

