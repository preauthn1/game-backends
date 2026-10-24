CREATE TABLE [Event].[Contest] (
    [ID]              INT          IDENTITY (1, 1) NOT NULL,
    [Name]            VARCHAR (50) NOT NULL,
    [GameID]          INT          NOT NULL,
    [StartTime]       DATETIME     NOT NULL,
    [EndTime]         DATETIME     NOT NULL,
    [Status]          TINYINT      NOT NULL,
    [Show]            TINYINT      NOT NULL,
    [Entertime]       TIME (7)     NOT NULL,
    [Deadline]        TIME (7)     NOT NULL,
    [PeriodEntertime] TIME (7)     NOT NULL,
    [PeriodDeadline]  TIME (7)     NOT NULL,
    [RewardAfterMin]  INT          NOT NULL,
    [RankInterval]    INT          CONSTRAINT [DF_Contest_Interval] DEFAULT ((0)) NOT NULL,
    [Charge]          BIGINT       NOT NULL,
    [PlayTime]        INT          NOT NULL,
    [Coin]            BIGINT       NOT NULL,
    [CreateTime]      DATETIME     CONSTRAINT [DF_Contest_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Contest] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否顯示', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'Show';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'入场截止时间', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'Deadline';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'結束後過幾分鐘計算獎勵', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'RewardAfterMin';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'區間結算週期, 如果是0表示不計算', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'RankInterval';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'入場費', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'Charge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'遊戲時間', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'PlayTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'比賽幣', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Contest', @level2type = N'COLUMN', @level2name = N'Coin';

