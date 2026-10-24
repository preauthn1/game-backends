CREATE TABLE [Event].[ContestUserData] (
    [GameID]      INT      NOT NULL,
    [UserID]      INT      NOT NULL,
    [ContestID]   INT      NOT NULL,
    [ContestNo]   INT      CONSTRAINT [DF_ContestUserData_ContestNo] DEFAULT ((0)) NOT NULL,
    [FlagID]      INT      NOT NULL,
    [TableNo]     INT      NOT NULL,
    [Playtime]    INT      CONSTRAINT [DF_ContestUserInfo_GameEndtime] DEFAULT ((99999)) NOT NULL,
    [RemainScore] BIGINT   CONSTRAINT [DF_ContestUserInfo_RemainScore] DEFAULT ((0)) NOT NULL,
    [Score]       BIGINT   CONSTRAINT [DF_ContestUserInfo_Score] DEFAULT ((0)) NOT NULL,
    [CreateTime]  DATETIME CONSTRAINT [DF_ContestUserInfo_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ContestUserData] PRIMARY KEY CLUSTERED ([GameID] ASC, [UserID] ASC, [ContestID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventContestUserInfo_UserID]
    ON [Event].[ContestUserData]([GameID] ASC, [UserID] ASC, [ContestID] ASC)
    INCLUDE([ContestNo]);

