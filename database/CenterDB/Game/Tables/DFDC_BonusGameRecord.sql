CREATE TABLE [Game].[DFDC_BonusGameRecord] (
    [ID]     BIGINT IDENTITY (1, 1) NOT NULL,
    [UserID] INT    NOT NULL,
    [TurnID] INT    NOT NULL,
    [Place]  INT    NOT NULL,
    [Icon]   INT    NOT NULL,
    CONSTRAINT [PK_DFDC_BonusGameRecord] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_DFDC_BonusGameRecord_UserID]
    ON [Game].[DFDC_BonusGameRecord]([UserID] ASC);

