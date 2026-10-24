CREATE TABLE [Stat].[UserBindLog] (
    [StatTime]       DATETIME NOT NULL,
    [PlatformID]     INT      NOT NULL,
    [AreaID]         INT      NOT NULL,
    [TotalBindCount] INT      NOT NULL,
    [SameDayReg]     INT      NOT NULL,
    [iOSCount]       INT      NOT NULL,
    [AndRoidCount]   INT      NOT NULL,
    [PCCount]        INT      NOT NULL,
    CONSTRAINT [PK_UserBindLog] PRIMARY KEY CLUSTERED ([StatTime] ASC, [PlatformID] ASC, [AreaID] ASC)
);

