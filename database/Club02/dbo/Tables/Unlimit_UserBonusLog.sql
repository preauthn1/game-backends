CREATE TABLE [dbo].[Unlimit_UserBonusLog] (
    [ID]                BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]           DATETIME NOT NULL,
    [LogHour]           INT      NOT NULL,
    [UserID]            INT      NOT NULL,
    [CreateTime]        DATETIME NOT NULL,
    [TotalPerformance]  BIGINT   NULL,
    [TeamPerformance]   BIGINT   NULL,
    [DirectPerformance] BIGINT   NULL,
    [TeamBonusAmount]   BIGINT   NULL,
    [DirectBonusAmount] BIGINT   NULL,
    [LevelLimit]        INT      NULL,
    [TopUserLevelLimit] INT      NULL,
    [RelationID]        BIGINT   NULL,
    CONSTRAINT [PK_Unlimit_UserBonusLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

