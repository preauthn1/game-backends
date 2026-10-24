CREATE TABLE [Event].[Reward] (
    [ID]        INT     IDENTITY (1, 1) NOT NULL,
    [ContestID] INT     NOT NULL,
    [Type]      TINYINT NOT NULL,
    [Rank]      INT     NOT NULL,
    [Money]     BIGINT  NOT NULL,
    CONSTRAINT [PK_Reward] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'排名', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Reward', @level2type = N'COLUMN', @level2name = N'Rank';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'贈送金幣', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Reward', @level2type = N'COLUMN', @level2name = N'Money';

