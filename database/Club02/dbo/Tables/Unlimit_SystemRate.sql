CREATE TABLE [dbo].[Unlimit_SystemRate] (
    [PlatformID]  INT          NOT NULL,
    [Level]       INT          NOT NULL,
    [LevelName]   VARCHAR (30) NOT NULL,
    [MinAmount]   BIGINT       NOT NULL,
    [MaxAmount]   BIGINT       NOT NULL,
    [BonusAmount] BIGINT       NOT NULL,
    CONSTRAINT [PK_Unlimit_SystemRate_1] PRIMARY KEY CLUSTERED ([PlatformID] ASC, [Level] ASC)
);

