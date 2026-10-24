CREATE TABLE [Users].[KeepLog] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT      CONSTRAINT [DF_KeepLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [AreaID]     INT      NULL,
    [RegCount]   INT      NULL,
    [StatTime]   DATETIME NOT NULL,
    [OneDayKeep] INT      NULL,
    [TwoDayKeep] INT      NULL,
    [WeekKeep]   INT      NULL,
    [MonthKeep]  INT      NULL,
    [Third]      INT      DEFAULT ((0)) NOT NULL,
    [Fourth]     INT      DEFAULT ((0)) NOT NULL,
    [Fifth]      INT      DEFAULT ((0)) NOT NULL,
    [Sixth]      INT      DEFAULT ((0)) NOT NULL,
    [eighth]     INT      DEFAULT ((0)) NOT NULL,
    [ninth]      INT      DEFAULT ((0)) NOT NULL,
    [tenth]      INT      DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_KeepLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

