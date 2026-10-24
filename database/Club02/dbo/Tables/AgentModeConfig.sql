CREATE TABLE [dbo].[AgentModeConfig] (
    [PlatformID] INT     NOT NULL,
    [AreaID]     INT     NOT NULL,
    [Mode1]      TINYINT NOT NULL,
    [Mode2]      TINYINT NOT NULL,
    [Mode3]      TINYINT NOT NULL,
    [Mode4]      TINYINT NOT NULL,
    [Mode5]      TINYINT NOT NULL,
    [Mode6]      TINYINT NOT NULL,
    CONSTRAINT [PK_AgentModeConfig] PRIMARY KEY CLUSTERED ([PlatformID] ASC, [AreaID] ASC)
);

