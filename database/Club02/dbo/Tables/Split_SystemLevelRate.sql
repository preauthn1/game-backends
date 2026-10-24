CREATE TABLE [dbo].[Split_SystemLevelRate] (
    [PlatformID]    INT    NOT NULL,
    [Level]         INT    NOT NULL,
    [MinRecruit1]   INT    NULL,
    [MinRecruit2]   INT    NULL,
    [MinRecruit3]   INT    NULL,
    [MaxRecruit1]   INT    NULL,
    [MaxRecruit2]   INT    NULL,
    [MaxRecruit3]   INT    NULL,
    [Stage1]        INT    NOT NULL,
    [Stage2]        INT    NOT NULL,
    [Stage3]        INT    NOT NULL,
    [TaxAmount]     BIGINT NOT NULL,
    [WithdrawLimit] INT    NOT NULL,
    CONSTRAINT [PK_Split_SystemLevelRate] PRIMARY KEY CLUSTERED ([PlatformID] ASC, [Level] ASC)
);

