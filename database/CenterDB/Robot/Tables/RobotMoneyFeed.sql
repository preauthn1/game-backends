CREATE TABLE [Robot].[RobotMoneyFeed] (
    [Id]     INT    IDENTITY (1, 1) NOT NULL,
    [UserID] INT    NOT NULL,
    [Amount] BIGINT NOT NULL,
    CONSTRAINT [PK_RobotMoneyFeed] PRIMARY KEY CLUSTERED ([Id] ASC)
);

