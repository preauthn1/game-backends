CREATE TABLE [Game].[SuperTool_Fish_PlayerPoolWinLost] (
    [FlagID]     INT    NOT NULL,
    [UserID]     INT    NOT NULL,
    [HitMoney]   BIGINT NOT NULL,
    [SpendMoney] BIGINT NOT NULL,
    CONSTRAINT [PK_SuperTool_Fish_PlayerPoolWinLost] PRIMARY KEY CLUSTERED ([FlagID] ASC, [UserID] ASC)
);

