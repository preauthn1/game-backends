CREATE TABLE [Event].[Score] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [ContestID]  INT      NOT NULL,
    [UserID]     INT      NOT NULL,
    [Score]      BIGINT   NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_Score_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Robot]      TINYINT  CONSTRAINT [DF_Score_Robot] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Score] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventScore_UserID]
    ON [Event].[Score]([ContestID] ASC, [UserID] ASC, [CreateTime] ASC)
    INCLUDE([Score]);

