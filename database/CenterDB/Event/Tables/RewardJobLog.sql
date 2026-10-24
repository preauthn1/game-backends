CREATE TABLE [Event].[RewardJobLog] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]   INT      NOT NULL,
    [Type]       TINYINT  NOT NULL,
    [GameID]     INT      NOT NULL,
    [ContestID]  INT      NOT NULL,
    [UserID]     INT      NOT NULL,
    [Rank]       INT      NOT NULL,
    [Score]      BIGINT   NOT NULL,
    [Reward]     BIGINT   NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_RewardJobLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_RewardJobLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

