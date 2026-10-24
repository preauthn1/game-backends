CREATE TABLE [Game].[UserGameInfo] (
    [Id]          INT    IDENTITY (1, 1) NOT NULL,
    [UserID]      INT    NOT NULL,
    [GameID]      INT    NOT NULL,
    [WinCount]    INT    NOT NULL,
    [LoseCount]   INT    NOT NULL,
    [DrawCount]   INT    NOT NULL,
    [Score]       BIGINT NOT NULL,
    [GameTime]    BIGINT NOT NULL,
    [LastWinLose] INT    DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_UserGameInfo] PRIMARY KEY CLUSTERED ([UserID] ASC, [GameID] ASC) ON [S_UserID] ([UserID])
) ON [S_UserID] ([UserID]);

