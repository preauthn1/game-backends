CREATE TABLE [Game].[GameWinMoney] (
    [UserID]   INT    NOT NULL,
    [GameType] INT    NOT NULL,
    [WinMoney] BIGINT NOT NULL,
    CONSTRAINT [PK_GameWinMoney_1] PRIMARY KEY CLUSTERED ([UserID] ASC, [GameType] ASC)
);

