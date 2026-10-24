CREATE TABLE [Event].[Ranking] (
    [ID]           INT      IDENTITY (1, 1) NOT NULL,
    [ContestID]    INT      NOT NULL,
    [UserID]       INT      NOT NULL,
    [CreateTime]   DATETIME NOT NULL,
    [Score]        INT      NOT NULL,
    [Rank]         INT      NOT NULL,
    [Type]         TINYINT  NOT NULL,
    [Period]       INT      NULL,
    [LastCalcRank] TINYINT  CONSTRAINT [DF_Ranking_LastCalcRank] DEFAULT ((0)) NOT NULL,
    [Reward]       BIGINT   NULL,
    CONSTRAINT [PK_Ranking] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventRanking_ContestID]
    ON [Event].[Ranking]([ContestID] ASC, [CreateTime] ASC)
    INCLUDE([UserID], [Score], [Rank]);


GO
CREATE NONCLUSTERED INDEX [IX_EventRanking_UserID]
    ON [Event].[Ranking]([ContestID] ASC, [UserID] ASC)
    INCLUDE([Rank]);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'日排名(1) 周排名(2)', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ranking', @level2type = N'COLUMN', @level2name = N'Type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'第幾個週期', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ranking', @level2type = N'COLUMN', @level2name = N'Period';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否已給予獎勵金額, 記錄金額', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ranking', @level2type = N'COLUMN', @level2name = N'Reward';

