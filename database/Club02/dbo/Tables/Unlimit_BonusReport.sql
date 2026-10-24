CREATE TABLE [dbo].[Unlimit_BonusReport] (
    [LogDate]           DATETIME NOT NULL,
    [UserID]            BIGINT   NOT NULL,
    [TotalPerformance]  BIGINT   NULL,
    [TeamPerformance]   BIGINT   NULL,
    [DirectPerformance] BIGINT   NULL,
    [TeamBonusAmount]   BIGINT   NULL,
    [DirectBonusAmount] BIGINT   NULL,
    [LevelLimit]        INT      NULL,
    [TopUserLevelLimit] INT      NULL,
    [Status]            INT      NOT NULL,
    CONSTRAINT [PK_Unlimit_BonusReport] PRIMARY KEY CLUSTERED ([UserID] ASC, [LogDate] ASC)
);

