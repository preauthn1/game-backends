CREATE TABLE [Game].[SuperTool_RateInfo] (
    [GameID] INT NOT NULL,
    [RateID] INT NOT NULL,
    [Value]  INT NOT NULL,
    CONSTRAINT [PK_SuperTool_RateInfo] PRIMARY KEY CLUSTERED ([GameID] ASC, [RateID] ASC)
);

