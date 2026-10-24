CREATE TABLE [Game].[WinAnnConfig] (
    [GameID]     INT    NOT NULL,
    [UpperLimit] BIGINT NOT NULL,
    CONSTRAINT [PK_WinLoseAnnConfig_GameID] PRIMARY KEY CLUSTERED ([GameID] ASC)
);

