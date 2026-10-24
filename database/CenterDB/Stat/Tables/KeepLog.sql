CREATE TABLE [Stat].[KeepLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT      CONSTRAINT [DF_KeepLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [RegCount]   INT      NOT NULL,
    [OneDayKeep] INT      NULL,
    [TwoDayKeep] INT      NULL,
    [WeekKeep]   INT      NULL,
    [MonthKeep]  INT      NULL,
    [StatTime]   DATETIME NOT NULL,
    [AreaID]     INT      NULL,
    [Third]      INT      DEFAULT ((0)) NOT NULL,
    [Fourth]     INT      DEFAULT ((0)) NOT NULL,
    [Fifth]      INT      DEFAULT ((0)) NOT NULL,
    [Sixth]      INT      DEFAULT ((0)) NOT NULL,
    [eighth]     INT      DEFAULT ((0)) NOT NULL,
    [ninth]      INT      DEFAULT ((0)) NOT NULL,
    [tenth]      INT      DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_KeepLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_KeepLog_StatTime]
    ON [Stat].[KeepLog]([StatTime] ASC);

